using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
using System.Collections;

namespace GPU_Lab.Utils
{
    public class Lab
    {      
      public static GPGPU _gpu = null;
      public const int N = 32 * 1024;
      public static int NN = 0;

      public static void InitGPU()
      {
        //CudafyModes.Target = eGPUType.Cuda;
        //CudafyModes.DeviceId = 0;
        //CudafyTranslator.Language = CudafyModes.Target == eGPUType.OpenCL ? eLanguage.OpenCL : eLanguage.Cuda;

        //int deviceCount = CudafyHost.GetDeviceCount(CudafyModes.Target);
        //if (deviceCount == 0)
        //  throw new InvalidOperationException(string.Format("No suitable {0} devices found.", CudafyModes.Target));
        CudafyTranslator.GenerateDebug = true;
                        
        CudafyModule _km = CudafyTranslator.Cudafy(typeof(Lab));
        _gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
        _gpu.LoadModule(_km);
        Console.WriteLine("Running examples using {0}", _gpu.GetDeviceProperties(false).Name);
      }

      public static void ExecDoSomeMath(int start, int end)
      {
        InitGPU();

        NN = end - start;
        double[] result = new double[NN];

        //Allocate GPU
        int[] dev_start = _gpu.Allocate<int>(start);
        int[] dev_end = _gpu.Allocate<int>(end);
        double[] dev_result = _gpu.Allocate<double>(result);

        _gpu.CopyToDevice(new int[1] { start }, dev_start);
        _gpu.CopyToDevice(new int[1] { end }, dev_end);

        _gpu.Launch(128, 1).DoSomeMath(dev_start, dev_end, dev_result);

        _gpu.CopyFromDevice(dev_result, result);

        _gpu.FreeAll();

        foreach (var r in result)
        {
          Console.WriteLine(r);
        }
      }

      public static void ExcecAdd()
      {
        InitGPU();

        int[] a = new int[N];
        int[] b = new int[N];
        int[] c = new int[N];

        //Allocate gpu mem
        int[] dev_a = _gpu.Allocate<int>(a);
        int[] dev_b = _gpu.Allocate<int>(b);
        int[] dev_c = _gpu.Allocate<int>(c);

        //Fill on CPU
        for (int i = 0; i < N; i++)
        {
          a[i] = i;
          b[i] = 2 * i;
        }

        //Copy to GPU
        _gpu.CopyToDevice(a, dev_a);
        _gpu.CopyToDevice(b, dev_b);

        _gpu.Launch(128, 1).add(dev_a, dev_b, dev_c);

        //Copy gpu -> cpu
        _gpu.CopyFromDevice(dev_c, c);

        //Verify
        for (int i = 0; i < N; i++)
        {
          if ((a[i] + b[i]) != c[i])
            throw new InvalidOperationException();
        }

        _gpu.Free(dev_a);
        _gpu.Free(dev_b);
        _gpu.Free(dev_c);

      }

      

      [Cudafy]
      public static void add(GThread thread, int[] a, int[] b, int[] c)
      {
        int tid = thread.blockIdx.x;
        while (tid < N)
        {
          c[tid] = a[tid] + b[tid];
          tid += thread.gridDim.x;
        }
      }

      //public static void calc(double a, double b)
      //{
      //  IList<double> aList = new List<double>();

      //  for (int i = 0; i < 10000000; i++)
      //  {
      //    var x = a / b + i;
      //    aList.Add(x);
      //  }
      //}

      [Cudafy()]
      public static void DoSomeMath(GThread thread, int[] start, int[] end, double[] result)
      {
        int tid = thread.blockIdx.x;
        int i = 0;
        int tot = end[0] - start[0];
        while (tid < N)
        {
          while (i < tot)
          {
            result[i] = Math.Sin((i + 3.14));
            i++;
          }
         
          tid += thread.gridDim.x;
        }
      }

      //[Cudafy]
      //public static void Primes(int num, int[] r)
      //{
      //  var primes = 
      //  Enumerable.Range(1, Convert.ToInt32(Math.Floor(Math.Sqrt(num))))
      //      .Aggregate(Enumerable.Range(1, num).ToList(),
      //      (result, index) =>
      //      {
      //        result.RemoveAll(i => i > result[index] && i % result[index] == 0);
      //        return result;
      //      }
      //      );
      //  int ii = 0;
      //  foreach (var prim in primes)
      //  {
      //    r[ii] = prim;
      //  }
      //}
    }
}
