using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayHouse.Utils;
using Serilog;

namespace SimpleStress;

internal class StressApplication
{
    private readonly List<TestClient> _clients = [];
    private Thread? _mainThreadAction;
    private readonly int _testCount = 0;
    private bool _isRunning = false;
    private LOG<StressApplication> _log = new();

    public StressApplication(int testCount)
    {
        _testCount = testCount;

        for (int i = 0; i < _testCount; ++i)
        {
            _clients.Add(new TestClient());
        }

        RunClientMainThread();
    }

    private void RunClientMainThread()
    {
        _isRunning = true;
        _mainThreadAction = new Thread(() =>
        {
            while (_isRunning)
            {
                foreach (var client in _clients)
                {
                    client.MainThreadAction();
                }

                Thread.Sleep(1);
            }
        });
        _mainThreadAction.Start();
    }

    private void StopClientMainThread()
    {
        _isRunning = false;
        _mainThreadAction!.Join(1000);

        _clients.Clear();
    }


    public void Prepare()
    {
        _log.Info(()=>$"================= Start Prepare ==================");
        var prePareTask = new Task[_testCount];
        for (var i = 0; i < _testCount; i++)
        {
            int index = i;
            prePareTask[i] = Task.Run(async () =>
            {
                await _clients[index].PrePareAsync();
            });
        }

        Task.WaitAll(prePareTask);
    }

    public void Run()
    {
        _log.Info(() => $"================= Start Run ==================");
        var runTask = new Task[_testCount];
        for (var i = 0; i < _testCount; i++)
        {
            int index = i;
            runTask[i] = Task.Run(async () =>
            {
                await _clients[index].RunAsync();
            });
        }

        Task.WaitAll(runTask);
    }

    public void Connect()
    {
        _log.Info(() => $"================= Start Connect ==================");
        var prePareTask = new Task[_testCount];
        for (var i = 0; i < _testCount; i++)
        {
            int index = i;
            prePareTask[i] = Task.Run(async () =>
            {
                await _clients[index].ConnectAsync();
            });
        }

        Task.WaitAll(prePareTask);
    }
}