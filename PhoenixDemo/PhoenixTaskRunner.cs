using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using ParallelProcessPractice.Core;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections;

namespace PhoenixDemo
{
	public class PhoenixTaskRunner : TaskRunnerBase
	{
		private BlockingCollection<MyTask> m_Pipe1 = new BlockingCollection<MyTask>();
		private BlockingCollection<MyTask> m_Pipe2 = new BlockingCollection<MyTask>();
		private BlockingCollection<MyTask> m_Pipe3 = new BlockingCollection<MyTask>();
		private SemaphoreSlim m_Semaphore1 = new SemaphoreSlim(5);
		private SemaphoreSlim m_Semaphore2 = new SemaphoreSlim(3);
		private SemaphoreSlim m_Semaphore3 = new SemaphoreSlim(3);

		public override void Run(IEnumerable<MyTask> tasks)
		{
			Method2(tasks);
		}

		private void Method1(IEnumerable<MyTask> tasks)
		{
			var pipe1 = Task.Run(() =>
			{
				var locked = new object();
				var count = 0;
				var completed = false;

				foreach (var t in tasks)
				{
					m_Semaphore1.Wait();

					lock (locked)
					{
						count++;
						Task.Run(() => t.DoStepN(1))
							.ContinueWith(_ =>
							{
								m_Semaphore2.Wait();
								m_Pipe2.Add(t);
								lock (locked)
									count--;

								if (count == 0 && completed)
									m_Pipe2.CompleteAdding();
								m_Semaphore1.Release();
							});
					}
				}
				completed = true;
			});
			var pipe2 = Task.Run(() =>
			{
				var locked = new object();
				var count = 0;
				var completed = false;

				foreach (var t in m_Pipe2.GetConsumingEnumerable())
				{
					lock (locked)
					{
						count++;
						Task.Run(() => t.DoStepN(2))
							.ContinueWith(_ =>
							{
								m_Semaphore3.Wait();

								m_Pipe3.Add(t);

								lock (locked)
									count--;

								if (count == 0 && completed)
									m_Pipe3.CompleteAdding();
								m_Semaphore2.Release();
							});
					}

				}

				completed = true;
			});

			var pipe3 = Task.Run(() =>
			{
				var locked = new object();
				var count = 0;
				var m = new ManualResetEvent(false);
				var completed = false;

				foreach (var t in m_Pipe3.GetConsumingEnumerable())
				{
					lock (locked)
					{
						count++;
						Task.Run(() => t.DoStepN(3))
						.ContinueWith(_ =>
						{
							m_Semaphore3.Release();

							lock (locked)
								count--;

							if (count == 0 && completed)
								m.Set();
						});
					}
				}

				completed = true;
				m.WaitOne();
			});

			pipe3.Wait();
		}

		private void Method2(IEnumerable<MyTask> tasks) => tasks.ToObservable()
					.Select(t =>
			{
				m_Semaphore1.Wait();

				return Task
					.Run(() => t.DoStepN(1))
					.ContinueWith(_ =>
					{
						m_Semaphore2.Wait();
						m_Semaphore1.Release();
						return t;
					});
			})
			.SelectMany(t => t)
			.Select(t =>
			{
				return Task
					.Run(() => t.DoStepN(2))
					.ContinueWith(_ =>
					{
						m_Semaphore3.Wait();
						m_Semaphore2.Release();
						return t;
					});
			})
			.SelectMany(t => t)
			.Select(t =>
			{
				return Task
					.Run(() => t.DoStepN(3))
					.ContinueWith(_ =>
					{
						m_Semaphore3.Release();
						return t;
					});
			})
			.SelectMany(t => t)
			.Wait();
	}

	public class PhoenixTaskRunner1 : TaskRunnerBase
	{
		private BlockingCollection<MyTask> m_Pipe1 = new BlockingCollection<MyTask>();
		private BlockingCollection<MyTask> m_Pipe2 = new BlockingCollection<MyTask>();
		private BlockingCollection<MyTask> m_Pipe3 = new BlockingCollection<MyTask>();
		private SemaphoreSlim m_Semaphore1 = new SemaphoreSlim(5);
		private SemaphoreSlim m_Semaphore2 = new SemaphoreSlim(3);
		private SemaphoreSlim m_Semaphore3 = new SemaphoreSlim(3);

		public override void Run(IEnumerable<MyTask> tasks)
		{
			Method1(tasks);
		}

		private void Method1(IEnumerable<MyTask> tasks)
		{
			var pipe1 = Task.Run(() =>
			{
				var locked = new object();
				var count = 0;
				var completed = false;

				foreach (var t in tasks)
				{
					m_Semaphore1.Wait();

					lock (locked)
					{
						count++;
						Task.Run(() => t.DoStepN(1))
							.ContinueWith(_ =>
							{
								m_Semaphore2.Wait();
								m_Pipe2.Add(t);
								lock (locked)
									count--;

								if (count == 0 && completed)
									m_Pipe2.CompleteAdding();
								m_Semaphore1.Release();
							});
					}
				}
				completed = true;
			});
			var pipe2 = Task.Run(() =>
			{
				var locked = new object();
				var count = 0;
				var completed = false;

				foreach (var t in m_Pipe2.GetConsumingEnumerable())
				{
					lock (locked)
					{
						count++;
						Task.Run(() => t.DoStepN(2))
							.ContinueWith(_ =>
							{
								m_Semaphore3.Wait();

								m_Pipe3.Add(t);

								lock (locked)
									count--;

								if (count == 0 && completed)
									m_Pipe3.CompleteAdding();
								m_Semaphore2.Release();
							});
					}

				}

				completed = true;
			});

			var pipe3 = Task.Run(() =>
			{
				var locked = new object();
				var count = 0;
				var m = new ManualResetEvent(false);
				var completed = false;

				foreach (var t in m_Pipe3.GetConsumingEnumerable())
				{
					lock (locked)
					{
						count++;
						Task.Run(() => t.DoStepN(3))
						.ContinueWith(_ =>
						{
							m_Semaphore3.Release();

							lock (locked)
								count--;

							if (count == 0 && completed)
								m.Set();
						});
					}
				}

				completed = true;
				m.WaitOne();
			});

			pipe3.Wait();
		}

		private void Method2(IEnumerable<MyTask> tasks) => tasks.ToObservable()
					.Select(t =>
					{
						m_Semaphore1.Wait();

						return Task
							.Run(() => t.DoStepN(1))
							.ContinueWith(_ =>
							{
								m_Semaphore2.Wait();
								m_Semaphore1.Release();
								return t;
							});
					})
			.SelectMany(t => t)
			.Select(t =>
			{
				return Task
					.Run(() => t.DoStepN(2))
					.ContinueWith(_ =>
					{
						m_Semaphore3.Wait();
						m_Semaphore2.Release();
						return t;
					});
			})
			.SelectMany(t => t)
			.Select(t =>
			{
				return Task
					.Run(() => t.DoStepN(3))
					.ContinueWith(_ =>
					{
						m_Semaphore3.Release();
						return t;
					});
			})
			.SelectMany(t => t)
			.Wait();
	}
}
