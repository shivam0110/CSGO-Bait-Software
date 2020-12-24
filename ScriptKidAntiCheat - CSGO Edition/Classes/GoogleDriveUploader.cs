using System;
using System.IO;
using System.Threading;
using System.IO.Compression;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using ScriptKidAntiCheat.Utils;
using System.Collections.Generic;

namespace ScriptKidAntiCheat
{
    public class GoogleDriveUploader
    {
        // If modifying these scopes, remember to generate new token
        static string[] Scopes = { DriveService.Scope.DriveFile };
        // ClientId & ClientSecret needs to be created at google developer console
        static readonly ClientSecrets secrets = new ClientSecrets()
        {
            ClientId = "",
            ClientSecret = ""
        };
        // Refresh token is generate by generateNewToken(); see line 41
        public string refreshToken = "";

        public GoogleDriveUploader()
        {
            if ((refreshToken == "" || secrets.ClientId == "" || secrets.ClientSecret == "") && Program.Debug.ShowDebugMessages)
            {
                System.Windows.Forms.MessageBox.Show("Google drive uploading is disabled - you need to create ClientId and ClientSecret in Google Developer Console. Check my GoogleDriveUploader class for more info.", "Developer helper", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        public void UploadFile(FileInfo ReplayFile)
        {
            if (refreshToken == "" || secrets.ClientId == "" || secrets.ClientSecret == "")
            {
                return;
            }

            // Generate new google drive token (saved in token.json)
            // Uncomment following line to generate new credentials for a google drive account (remember to comment out the predefined refresh token on line 31 first)
            // UserCredential credential =  generateNewToken();

            // Authorize with predefined RefreshToken (RefreshTokens never expire on it's own)
            UserCredential credential = AuthorizeWithRefreshToken(refreshToken);

            Log.AddEntry(new LogEntry()
            {
                LogTypes = new List<LogTypes> { LogTypes.Analytics },
                AnalyticsCategory = "Replays",
                AnalyticsAction = "ZipDirectory"
            });

            // Zip directory before uploading to google drive
            string zipFile = ZipDirectory(ReplayFile);

            // Make sure zip was successful before proceeding
            if (!File.Exists(zipFile))
            {
                return;
            }

            Log.AddEntry(new LogEntry()
            {
                LogTypes = new List<LogTypes> { LogTypes.Analytics },
                AnalyticsCategory = "Replays",
                AnalyticsAction = "ZipDirectorySuccess"
            });

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "ProjectNeuron",
            });

            // File information for google drive
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = ReplayFile.Name + ".zip",
                MimeType = "application/zip, application/octet-stream, application/x-zip-compressed, multipart/x-zip"
            };

            FilesResource.CreateMediaUpload request;

            Log.AddEntry(new LogEntry()
            {
                LogTypes = new List<LogTypes> { LogTypes.Analytics },
                AnalyticsCategory = "Replays",
                AnalyticsAction = "UploadStart"
            });

            // Do the actual file upload to google drive
            using (var stream = new System.IO.FileStream(zipFile, System.IO.FileMode.Open))
            {
                request = service.Files.Create(fileMetadata, stream, "application/zip");
                request.Fields = "id";
                request.Upload();
            }

            // Recieve the response from google drive upload
            var file = request.ResponseBody;

            if(Program.Debug.ShowDebugMessages)
            {
                if (file != null && file.Id.Length > 0)
                {
                    System.Windows.Forms.MessageBox.Show("Upload complete", "Debug", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Upload failed", "Debug", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            } else
            {
                if (file != null && file.Id.Length > 0)
                {
                    Log.AddEntry(new LogEntry()
                    {
                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                        AnalyticsCategory = "Replays",
                        AnalyticsAction = "UploadSuccess"
                    });
                }
                else
                {
                    Log.AddEntry(new LogEntry()
                    {
                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                        AnalyticsCategory = "Replays",
                        AnalyticsAction = "UploadFail"
                    });
                }
            }

            // Cleanup after upload
            if (File.Exists(zipFile))
            {
                File.Delete(zipFile);  // Delete zip file
            }
        }

        public UserCredential generateNewToken()
        {
            UserCredential credential;

            // Delete existing token directory (saved where program is run from)
            if (Directory.Exists("token.json"))
            {
                Directory.Delete("token.json", true);
            }

            // Generate new credentials (will open google drive login in browser)
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new GoogleAuthorizationCodeFlow.Initializer { ClientSecrets = secrets },
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("token.json", true)).Result;

            // Return credentials after signin
            return credential;
        }

        private UserCredential AuthorizeWithRefreshToken(string token)
        {
            UserCredential credential;

            // Get existing credentials using RefreshToken (can be found inside token.json after generating new token)
            credential = new UserCredential(
                new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer { ClientSecrets = secrets }
                ),
                "user",
                new TokenResponse
                {
                    RefreshToken = token
                });

            // Return credentials
            return credential;
        }

        private string ZipDirectory(FileInfo ReplayFile)
        {
            string zipPath = Path.GetTempPath() + ReplayFile.Name + ".zip";
            string cleanReplayName = ReplayFile.Name.Replace("#sheeter", "");
            string replayTmpDirPath = Path.GetTempPath() + cleanReplayName;
            string logFilePath = ReplayFile.DirectoryName + @"\" + Path.GetFileNameWithoutExtension(ReplayFile.FullName) + ".log";
            string manifestFilePath = ReplayFile.DirectoryName + @"\" + Path.GetFileNameWithoutExtension(ReplayFile.FullName) + ".manifest.log";

            // Create temporary dir where we will place replay file
            Directory.CreateDirectory(replayTmpDirPath);

            // Move Replay log if it exists
            if (File.Exists(logFilePath))
            {
                FileInfo ReplayLogFile = new FileInfo(logFilePath);
                string cleanLogFileName = ReplayLogFile.Name.Replace("#sheeter", "");
                ReplayLogFile.MoveTo(replayTmpDirPath + @"\" + cleanLogFileName);
            }

            // Move Replay manifest if it exists
            if (File.Exists(manifestFilePath))
            {
                FileInfo ReplayManifestFile = new FileInfo(manifestFilePath);
                string cleanManifestFileName = ReplayManifestFile.Name.Replace("#sheeter", "");
                ReplayManifestFile.MoveTo(replayTmpDirPath + @"\" + cleanManifestFileName);
            }

            // Move replay file to tmp dir
            ReplayFile.MoveTo(replayTmpDirPath + @"\" + cleanReplayName);

            if (!File.Exists(zipPath) && Directory.Exists(replayTmpDirPath) && File.Exists(replayTmpDirPath + @"\" + cleanReplayName))
            {
                // Zip Directory
                ZipFile.CreateFromDirectory(replayTmpDirPath, zipPath);
            }

            return zipPath;
        }
    }
}
