﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB_Live
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            App app = new App();
            app.InitializeComponent();

            return app.Run();
        }
    }
}
