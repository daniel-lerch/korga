﻿using McMaster.Extensions.CommandLineUtils;

#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Server.Commands
{
    [Command("korga", Description = "Korga server management console")]
    [Subcommand(typeof(DatabaseCommand))]
    public class KorgaCommand
    {
        private int OnExecute(CommandLineApplication app)
        {
            app.ShowHint();
            return 1;
        }
    }
}
