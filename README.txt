Hi! :)

Read this document if you want to understand how everything works.

Disclaimer:
This project is written in C# and is 100% external without memory writing. 
The main reason for me to share the source code is to serve as evidence 
that my videos are not faked. If you have come here to learn programming 
this is not a good place to start and you should google real tutorials 
instead. This project is poorly documented, poorly written with a lot of 
hacky solutions.

MemoryReading
This project reads the memory to get information from the game. 
Current memory offsets are grabed from my repo on program start.
I try keeping the offsets up to date with every game update but feel free to load
your own offsets by creating file /offsets/offsets.txt

MemoryWriting
This project _do not_ rely on memory writing at all. There was simply no need
to write memory to accomplish what was needed for the purpose of this bait software.

How do I send console commands without writing memory?
the fake cheat adds "netconport" to csgo launch options which allows direct telnet access to read & write
from console (built in functionality in source engine).

Where is the code for DoYouEvenAimBro or BloodBrothers or RoflCopter?
I have removed these punishments from the public source code because I don't want to support
the spread of code that can easily be changed into real cheats. By changing 1 line of code 
in those punishments they could have been turned into a working triggerbot/aimbot.

Debugging
In program.cs I have prepared debugging settings and turned off running the process in background
when you close the main window. Play around with the debugging settings or remove all of them to
make it run like it would for a real cheater.

Third party stuff
This project uses third party references like google drive api, sharpdx, costura and more.
I have also tried to credit the authors in the classes where I have used copy paste code or
based code on someone else code or tutorial.

How do I create tripwires?
I explain this in this video: https://youtu.be/CNWkq7NH1g8?t=260

// ScriptKid
