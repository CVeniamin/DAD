﻿using System;
using System.IO;
using System.Text;

namespace OGP.Client
{
    internal class SuppressedWriter : TextWriter
    {
        private TextWriter originalOut;

        public SuppressedWriter()
        {
            originalOut = Console.Out;
        }

        public override void Write(string value)
        {
            Console.Error.WriteLine(value);
        }

        public override void Write(string value, object arg0)
        {
            if ((string)arg0 == "PCS_REPLY")
            {
                originalOut.WriteLine(value);
            }
            else
            {
                Console.Error.WriteLine(value, arg0);
            }
        }

        public override Encoding Encoding => Encoding.ASCII;
    }
}