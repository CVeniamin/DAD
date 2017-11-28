using Sprache;
using System;

namespace OGP.PuppetMaster
{
    internal class CommandParser
    {
        private static readonly Parser<string> PID = Parse.LetterOrDigit.AtLeastOnce().Text().Token();
        private static readonly Parser<string> URL = Parse.CharExcept(" ").AtLeastOnce().Text().Token();
        private static readonly Parser<string> Number = Parse.Digit.AtLeastOnce().Text().Token();

        private static readonly Parser<ICommand> StartClient =
            (from cmd in Parse.String("StartClient").Token()
             from pid in PID
             from pcsURL in URL
             from clientURL in URL
             from msPerRound in Number
             from numPlayers in Number
             from filename in Parse.Optional(URL)
             select (ICommand)new StartClient(pid, pcsURL, clientURL, Int32.Parse(msPerRound), Int32.Parse(numPlayers), filename.IsDefined ? (string)filename.Get() : null));

        private static readonly Parser<ICommand> StartServer =
            (from cmd in Parse.String("StartServer").Token()
             from pid in PID
             from pcsURL in URL
             from serverURL in URL
             from msPerRound in Number
             from numPlayers in Number
             select (ICommand)new StartServer(pid, pcsURL, serverURL, Int32.Parse(msPerRound), Int32.Parse(numPlayers)));

        private static readonly Parser<ICommand> GlobalStatus =
            (from cmd in Parse.String("GlobalStatus").Token()
             select (ICommand)new GlobalStatus());

        private static readonly Parser<ICommand> Crash =
            (from cmd in Parse.String("Crash").Token()
             from pid in PID
             select (ICommand)new Crash(pid));

        private static readonly Parser<ICommand> Freeze =
            (from cmd in Parse.String("Freeze").Token()
             from pid in PID
             select (ICommand)new Freeze(pid));

        private static readonly Parser<ICommand> Unfreeze =
            (from cmd in Parse.String("Unfreeze").Token()
             from pid in PID
             select (ICommand)new Unfreeze(pid));

        private static readonly Parser<ICommand> InjectDelay =
            (from cmd in Parse.String("InjectDelay").Token()
             from srcPid in PID
             from dstPid in PID
             select (ICommand)new InjectDelay(srcPid, dstPid));

        private static readonly Parser<ICommand> LocalState =
            (from cmd in Parse.String("LocalState").Token()
             from pid in PID
             from roundID in Number
             select (ICommand)new LocalState(pid, Int32.Parse(roundID)));

        private static readonly Parser<ICommand> Wait =
            (from cmd in Parse.String("Wait").Token()
             from ms in Number
             select (ICommand)new Wait(Int32.Parse(ms)));

        public static Parser<ICommand> Command = (StartClient).Or(StartServer)
            .Or(GlobalStatus).Or(Crash).Or(Freeze).Or(Unfreeze).Or(InjectDelay)
            .Or(LocalState).Or(Wait);
    }
}