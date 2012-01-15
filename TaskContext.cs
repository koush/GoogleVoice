using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;

namespace GoogleVoice
{
	public class TaskContext
	{
		void Continue()
		{
			if (mException != null || !mEnumerator.MoveNext())
			{
				mAction();
			}
			else
			{
				// we have yielded a result, so let's trigger the action to trigger the next result.
				mEnumerator.Current();
			}
		}

		Action mCompletionHandler;
		public Action TaskCompletionHandler
		{
			get
			{
				return mCompletionHandler ?? (mCompletionHandler = new Action(Continue));
			}
		}

		IEnumerable<Action> mTasks;
		IEnumerator<Action> mEnumerator;
		Action mAction;
		public void Attach(IEnumerable<Action> tasks, Action a)
		{
			mAction = a;
			mTasks = tasks;
			mEnumerator = mTasks.GetEnumerator();
			Continue();
		}

		Exception mException;
		public void SetException(Exception e)
		{
			mException = e;
		}
	}
}
