using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPU_Lab
{
  class Program
  {
    [STAThread]
    static void Main(string[] args)
    {
      //Utils.Lab.ExcecAdd();
      //Finish();

      Utils.Lab.ExecDoSomeMath(1, 100000);
      Finish();

      
    }

    static void Finish()
    {
      Console.WriteLine("Finished.");
      Console.ReadLine();
    }
  }
}
