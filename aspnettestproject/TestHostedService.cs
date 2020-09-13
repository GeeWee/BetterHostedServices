// using System;
// using System.Runtime.InteropServices;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Hosting;
//
// namespace aspnettestproject
// {
//
//
//     public class TestHostedService: CriticalBackgroundService
//     {
//
//         // The goal is to get this to fail
//         protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//         {
//             Console.WriteLine("EXECUTE ASYNC");
//             await Task.Yield();
//             throw new NotImplementedException();
//         }
//
//         // // This will fail silently
//         // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//         // {
//         //     Console.WriteLine("EXECUTE ASYNC");
//         //     await Task.Yield();
//         //     throw new NotImplementedException();
//         // }
//
//
//         // // This will fail on startup
//         // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//         // {
//         //     Console.WriteLine("EXECUTE ASYNC");
//         //     await Task.CompletedTask;
//         //     throw new NotImplementedException();
//         // }
//     }
// }
