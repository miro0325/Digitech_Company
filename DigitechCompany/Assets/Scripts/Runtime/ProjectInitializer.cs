using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class TaskData
{
    public abstract UniTask Task();
}

public class LoadingTaskData : TaskData
{
    public LoadingTaskData(Func<UniTask> task, Action onComplete = null)
    {
        this.task = UniTask.Defer(task);
        this.onComplete = onComplete;
    }

    public override async UniTask Task()
    {
        await task;
        onComplete?.Invoke();
    }

    public UniTask task;
    public Action onComplete;
}

public class LoadingTaskData<T> : TaskData
{
    public LoadingTaskData(Func<UniTask<T>> task, Action<T> onComplete = null)
    {
        this.task = UniTask.Defer(task);
        this.onComplete = onComplete;
    }

    public override async UniTask Task()
    {
        onComplete?.Invoke(await task);
    }

    public UniTask<T> task;
    public Action<T> onComplete;
}

public class ProjectInitializer : MonoBehaviour
{
    private List<TaskData> loadingTasks = new();

    public event Action OnLoadComplete;
    public bool IsEndLoad { get; private set; }
    public float Progress { get; private set; }

    public void AddTask(TaskData taskData)
        => loadingTasks.Add(taskData);

    public void Awake()
    {
        Services.Register(this);
        InvokeTask();
    }

    public void InvokeTask()
    {
        InvokeTasks().Forget();
    }

    private async UniTaskVoid InvokeTasks()
    {
        await UniTask.WaitForSeconds(0.25f);

        var count = 0;
        foreach (var taskData in loadingTasks)
        {
            await taskData.Task();
            count++;
            Progress = (float)count / loadingTasks.Count;
        }
        OnLoadComplete?.Invoke();
        IsEndLoad = true;
    }
}