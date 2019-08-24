# Parallel Process Practices

目標: 請用最少的時間, 完成 1000 個 MyTask instances 的三個步驟。
限制: 每個 MyTask 的同一個步驟，都無法併行處理。

# 作法

1. 請建立你自己的 Console App, 參考 ParallelProcessPractice.Core 專案
1. 建立你自己的衍生類別: 
```csharp

public class AndrewTaskRunner : TaskRunnerBase
{
    public override void Run(IEnumerable<MyTask> tasks)
    {
        foreach (var t in tasks)
        {
            t.DoStep1();
            t.DoStep2();
            t.DoStep3();
            Console.WriteLine($"exec job: {t.ID} completed.");
        }
    }
}

```

1. 執行你的 test:
```csharp

static void Main(string[] args)
{
    TaskRunnerBase run = new AndrewTaskRunner();
    run.ExecuteTasks(10);
}

```

2. 執行結果:

```text

Execution Summary: PASS

* Max WIP:
  - ALL:      1
  - Step #1:  1
  - Step #2:  1
  - Step #3:  1

* Execute Time:
  - Time To First Task Completed: 614.3636 msec
  - Time To Last Task Completed:  6047.8443 msec

* Execute Count:
  - Total:   10
  - Success: 10
  - Failure: 0

```

# 評比標準:

只有第一行顯示 PASS 才算通過。顯示 FAIL 則所有數據均不承認。詳細資訊可以從 ```Execute Count``` 得知詳細的成功與失敗數量。

執行效率會參考兩個指標，第一優先的指標是 WIP, 第二優先的指標是 ```Execute Time```。

* 半成品 WIP (Work In Progress):  
> 代表同時間在處理中的 Task 總數。處理中的 Task 會需要耗用資源 (記憶體，資料庫連線，CPU 等等)，因此好的 code 應能妥善控制 WIP 的數量，數字越少越好。

* 執行時間 (Execute Time):  
> 代表執行花費的總時間，越少越好。

指標參考的優先順序 (由高到低):

1. WIP (ALL)
1. Execute Time (FIRST TASK)
1. Execute Time (TOTAL TASK)
1. WIP (STEP1)
1. WIP (STEP2)
1. WIP (STEP3)

# Task 特徵設定

這個練習專案的 Task 有幾個前提, 例如總共有 3 個步驟 (Step), 每個步驟有既定的執行時間 (Duration), 也有並行處理的限制。
這些設定都集中在 PracticeSettings.cs 內。若需要調整組態來模擬不同情境，可以直接調整:

```csharp

internal static class PracticeSettings
{
    public const int TASK_TOTAL_STEPS = 3;

    public static readonly int[] TASK_STEPS_DURATION =
    {
        0,  // STEP 0, useless
        300,
        100,
        200
    };

    public static readonly int[] TASK_STEPS_CONCURRENT_LIMIT =
    {
        0,  // STEP 0, useless
        1,
        1,
        1
    };
}

```

其中:

* ```TASK_TOTAL_STEPS``` 代表總共有多少 STEP 需要處理。
* ```TASK_STEPS_DURATION``` 代表每個 STEP 處理需要花費的時間 (msec), 為了簡化設計, 這個陣列設定值 indexer 是從 1 開始。因此第一筆 [0] 會被忽略不計。
* ```TASK_STEPS_CONCURRENT_LIMIT``` 代表每個 STEP 允許最大同時處理的數量。同上, 第一筆 [0] 會被忽略不計。