using Sprache;
using System;

namespace OGP.Server
{
    internal class CommandParser
    {
        private static readonly Parser<string> Number = Parse.Digit.AtLeastOnce().Text().Token();
        
        private static readonly Parser<ICommand> GlobalStatus =
            (from cmd in Parse.String("GlobalStatus").Token()
             select (ICommand)new GlobalStatus());

        private static readonly Parser<ICommand> LocalState =
            (from cmd in Parse.String("LocalState").Token()
             from roundId in Number
             select (ICommand)new LocalState(Int32.Parse(roundId)));

        private static readonly Parser<ICommand> InjectDelay =
            (from cmd in Parse.String("InjectDelay").Token()
             from dstPid in Number
             select (ICommand)new InjectDelay(dstPid));

        private static readonly Parser<ICommand> Unfreeze =
            (from cmd in Parse.String("Unfreeze").Token()
             select (ICommand)new Unfreeze());

        private static readonly Parser<ICommand> Freeze =
            (from cmd in Parse.String("Freeze").Token()
             select (ICommand)new Freeze());
        
        public static Parser<ICommand> Command = (GlobalStatus).Or(LocalState)
            .Or(InjectDelay).Or(Unfreeze).Or(Freeze);
    }
}