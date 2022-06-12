using System ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using EaseHelper ;

namespace Template
{
	/// <summary>
	/// MonoBehaviour のメソッド拡張 Version 2022/06/12
	/// </summary>
	public class ExMonoBehaviour : MonoBehaviour
	{
		[Header("<Override ExMonoBehaviour>")]

		[SerializeField]
		protected bool					m_DisableTaskCancellation = false ;

		//-----------------------------------------------------------

		private CancellationTokenSource	m_ActiveCancellationTokenSource = null ;
		private bool m_IsRegistedDestroyCallback = false ;

		// 既に GameObject が削除されてしまったか
		private	bool					m_GameObjectWasDestroyed = false ;

		// キャンセルトークンを取得する
		private CancellationToken GetActiveCancellationToken( CancellationToken cancellationToken )
		{
			if( m_DisableTaskCancellation == true ){ return cancellationToken ; }

			CancellationToken ownerCancellationToken = this.GetCancellationTokenOnDestroy() ;
			if( m_IsRegistedDestroyCallback == false )
			{
				ownerCancellationToken.Register( () =>
				{
					if( m_ActiveCancellationTokenSource != null )
					{
						m_ActiveCancellationTokenSource.Dispose() ;
						m_ActiveCancellationTokenSource = null ;
					}

					m_GameObjectWasDestroyed = true ;	// GameObject が削除された状態になっている
				} ) ;
				m_IsRegistedDestroyCallback  = true ; 
			}

			if( cancellationToken == default || cancellationToken.CanBeCanceled == false )
			{
				// 基本のキャンセルトークン生成

				if( m_ActiveCancellationTokenSource != null && m_ActiveCancellationTokenSource.IsCancellationRequested == true )
				{
					// 既にキャンセル済みのトークンが使われていたら作り直す
					m_ActiveCancellationTokenSource.Dispose() ;
					m_ActiveCancellationTokenSource = null ;
				}

				if( m_ActiveCancellationTokenSource == null )
				{
					m_ActiveCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( ownerCancellationToken ) ;
				}
			}
			else
			{
				// 独自のキャンセルトークン生成

				if( m_ActiveCancellationTokenSource != null )
				{
					// 使用の有無に関わらずトークンは作り直す
					m_ActiveCancellationTokenSource.Dispose() ;
					m_ActiveCancellationTokenSource = null ;
				}

				m_ActiveCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken, ownerCancellationToken ) ;
			}

			return m_ActiveCancellationTokenSource.Token ;
		}

		/// <summary>
		/// 実行中のタスクを中断する
		/// </summary>
		/// <returns></returns>
		public bool CancelTask()
		{
			if( m_ActiveCancellationTokenSource == null )
			{
				return false ;	// タスクは実行していない
			}

			if( m_ActiveCancellationTokenSource.Token.CanBeCanceled == true )
			{
				m_ActiveCancellationTokenSource.Cancel() ;
			}

			m_ActiveCancellationTokenSource.Dispose() ;
			m_ActiveCancellationTokenSource = null ;

			return true ;		// 実行中のタスクは中断されたはず
		}

		//-------------------------------------------------------------------------------------------
		// 待機メソッド群

		/// <summary>
		/// １フレーム分だけ待つ
		/// </summary>
		/// <param name="timing"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public UniTask Yield( PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return UniTask.Yield( timing, GetActiveCancellationToken( cancellationToken ) ) ;
		}

		/// <summary>
		/// 指定した時間だけ待つ
		/// </summary>
		/// <param name="time"></param>
		/// <param name="ignoreTimeScale"></param>
		/// <param name="delayTiming"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public UniTask Delay( int millisecondsDelay, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			var delayTimeSpan = TimeSpan.FromMilliseconds( millisecondsDelay ) ;
			return UniTask.Delay( delayTimeSpan, ignoreTimeScale, delayTiming, GetActiveCancellationToken( cancellationToken ) ) ;
		}

		/// <summary>
		/// 指定したフレーム数だけ待つ
		/// </summary>
		/// <param name="delayFrameCount"></param>
		/// <param name="ignoreTimeScale"></param>
		/// <param name="delayTiming"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public UniTask DelayFrame( int delayFrameCount, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return UniTask.DelayFrame( delayFrameCount, delayTiming, GetActiveCancellationToken( cancellationToken ) ) ;
		}

		/// <summary>
		/// フレームの終わりまで待機する
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public UniTask WaitForFixedUpdate( CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return UniTask.WaitForFixedUpdate( GetActiveCancellationToken( cancellationToken ) ) ;
		}

		/// <summary>
		/// 指定した時間だけ待つ
		/// </summary>
		/// <param name="time"></param>
		/// <param name="ignoreTimeScale"></param>
		/// <param name="delayTiming"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public UniTask WaitForSeconds( float time, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			var delayTimeSpan = TimeSpan.FromMilliseconds( time * 1000 ) ;
			return UniTask.Delay( delayTimeSpan, ignoreTimeScale, delayTiming, GetActiveCancellationToken( cancellationToken ) ) ;
		}

		/// <summary>
		/// 条件が満たされてる間(true)は待機する
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public UniTask WaitWhile( Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return UniTask.WaitWhile( predicate, timing, GetActiveCancellationToken( cancellationToken ) ) ;
		}

		/// <summary>
		/// 条件が満たされるまでの間(false)は待機する
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public UniTask WaitUntil( Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return UniTask.WaitUntil( predicate, timing, GetActiveCancellationToken( cancellationToken ) ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コールバック用の時間経過チェックメソッド
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		protected Func<bool> WaitForSeconds_NotTask( float duration )
		{
			float startTime = Time.realtimeSinceStartup ;

			bool IsOver()
			{
				return ( Time.realtimeSinceStartup - startTime ) >= duration ;
			} ;

			//----------------------------------------------------------

			return IsOver ;
		}

		/// <summary>
		/// いずれか１つのタスクが完了になるまで待機する(全て未実行状態では待機しない)　※基本的に実行中のタスクのみ引数に渡すようにしてください
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public UniTask WhenAny( params System.Object[] tasks )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return WhenAny( default, tasks ) ;
		}

		/// <summary>
		/// いずれか１つのタスクが完了になるまで待機する(全て未実行状態では待機しない)　※基本的に実行中のタスクのみ引数に渡すようにしてください
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public UniTask WhenAny( List<System.Object> tasks )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return WhenAny( default, tasks.ToArray() ) ;
		}

		/// <summary>
		/// いずれか１つのタスクが完了になるまで待機する(全て未実行状態では待機しない)　※基本的に実行中のタスクのみ引数に渡すようにしてください
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public async UniTask WhenAny( CancellationToken cancellationToken, params System.Object[] tasks )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			// OnDestroy() 実行後の場合はリーク対策として強制的にキャンセルをかける
			cancellationToken = GetActiveCancellationToken( cancellationToken ) ;

			//----------------------------------

			// 重要
			// 判定対象のタスクは実行中以外(失敗・中断・終了)は全て終了扱いとする
			// よって判定対象のタスクが中断された場合のオーナータスクを含めた一括中断は行われない

			int countMax ;
			int countNow ;

			while( true )
			{
				countMax = 0 ;
				countNow = 0 ;

				foreach( var task in tasks )
				{
					if( task != null )
					{
						if( task is List<Task> )
						{
							// List<Task>
							List<Task> s_tasks = task as List<Task> ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null )
								{
									countMax ++ ;	// 実行中のタスク数
									if( s_task.Status != TaskStatus.Running )
									{
										countNow ++ ;	// 完了
									}
								}
							}
						}
						else
						if( task is List<UniTask> )
						{
							// List<UniTask>
							List<UniTask> s_tasks = task as List<UniTask> ;
							foreach( var s_task in s_tasks )
							{
								countMax ++ ;
								if( s_task.Status != UniTaskStatus.Pending )
								{
									countNow ++ ;	// 停止中
								}
							}
						}
						else
						if( task is List<CustomYieldInstruction> )
						{
							// List<CustomYieldInstruction>
							List<CustomYieldInstruction> s_tasks = task as List<CustomYieldInstruction> ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null )
								{
									countMax ++ ;
									if( s_task.keepWaiting != true )
									{
										countNow ++ ;	// 停止中
									}
								}
							}
						}
						else
						if( task is Task[] )
						{
							// Task[]
							Task[] s_tasks = task as Task[] ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null )
								{
									countMax ++ ;
									if( s_task.Status != TaskStatus.Running )
									{
										countNow ++ ;	// 停止中
									}
								}
							}
						}
						else
						if( task is UniTask[] )
						{
							// UniTask[]
							UniTask[] s_tasks = task as UniTask[] ;
							foreach( var s_task in s_tasks )
							{
								countMax ++ ;
								if( s_task.Status != UniTaskStatus.Pending )
								{
									countNow ++ ;	// 停止中
								}
							}
						}
						else
						if( task is CustomYieldInstruction[] )
						{
							// CustomYieldInstruction[]
							CustomYieldInstruction[] s_tasks = task as CustomYieldInstruction[] ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null )
								{
									countMax ++ ;
									if( s_task.keepWaiting != true )
									{
										countNow ++ ;	// 停止中
									}
								}
							}
						}
						else
						if( task is Task _1 )
						{
							// Task
							countMax ++ ;
							if( _1.Status != TaskStatus.Running )
							{
								countNow ++ ;	// 停止中
							}
						}
						else
						if( task is UniTask _2 )
						{
							// UniTask
							countMax ++ ;
							if( _2.Status != UniTaskStatus.Pending )
							{
								countNow ++ ;	// 停止中
							}
						}
						else
						if( task is UniTask<bool>	_201 ){ countMax ++ ; if( _201.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<byte>	_202 ){ countMax ++ ; if( _202.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<char>	_203 ){ countMax ++ ; if( _203.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<short>	_204 ){ countMax ++ ; if( _204.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<ushort>	_205 ){ countMax ++ ; if( _205.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<int>	_206 ){ countMax ++ ; if( _206.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<uint>	_207 ){ countMax ++ ; if( _207.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<long>	_208 ){ countMax ++ ; if( _208.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<ulong>	_209 ){ countMax ++ ; if( _209.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<float>	_210 ){ countMax ++ ; if( _210.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<double>	_211 ){ countMax ++ ; if( _211.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<string>	_212 ){ countMax ++ ; if( _212.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						else
						if( task is CustomYieldInstruction _3 )
						{
							// CustomYieldInstruction
							countMax ++ ;
							if( _3.keepWaiting != true )
							{
								countNow ++ ;	// 停止中
							}
						}
						else
						if( task is Func<bool> _4 )
						{
							// Func<bool>
							countMax ++ ;
							if( _4() != false )
							{
								countNow ++ ;	// 停止中
							}
						}
					}
				}

				await UniTask.Yield( PlayerLoopTiming.Update, cancellationToken ) ;

				if( countMax == 0 || countNow >  0 )
				{
					// 実行されているタスクは無いか１つ以上終了した
					break ;
				}
			}
		}

		/// <summary>
		/// いずれか１つのタスクが完了になるまで待機する(全て未実行状態では待機しない)　※基本的に実行中のタスクのみ引数に渡すようにしてください
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public UniTask WhenAny( params Func<bool>[] funcs )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return WhenAny( default, funcs ) ;
		}

		/// <summary>
		/// いずれか１つのタスクが完了になるまで待機する(全て未実行状態では待機しない)　※基本的に実行中のタスクのみ引数に渡すようにしてください
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public async UniTask WhenAny( CancellationToken cancellationToken, params Func<bool>[] funcs )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			// OnDestroy() 実行後の場合はリーク対策として強制的にキャンセルをかける
			cancellationToken = GetActiveCancellationToken( cancellationToken ) ;

			//----------------------------------

			// 重要
			// 判定対象のタスクは実行中以外(失敗・中断・終了)は全て終了扱いとする
			// よって判定対象のタスクが中断された場合のオーナータスクを含めた一括中断は行われない

			int countMax ;
			int countNow ;

			while( true )
			{
				countMax = 0 ;
				countNow = 0 ;

				foreach( var func in funcs )
				{
					if( func != null )
					{
						if( func is Func<bool> _1 )
						{
							// Func<bool>
							countMax ++ ;
							if( _1() == true )
							{
								countNow ++ ;		// 完了中
							}
						}
					}
				}

				await UniTask.Yield( PlayerLoopTiming.Update, cancellationToken ) ;

				if( countMax == 0 || countNow >  0 )
				{
					// 実行されているタスクは無いか１つ以上終了した
					break ;
				}
			}
		}

		/// <summary>
		/// 全ての非同期処理の終了を待つ
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public UniTask WhenAll( params System.Object[] tasks )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return WhenAll( default, tasks ) ;
		}

		/// <summary>
		/// 全ての非同期処理の終了を待つ
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public UniTask WhenAll( List<System.Object> tasks )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return WhenAll( default, tasks.ToArray() ) ;
		}

		/// <summary>
		/// 全ての非同期処理の終了を待つ
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public async UniTask WhenAll( CancellationToken cancellationToken, params System.Object[] tasks )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			// OnDestroy() 実行後の場合はリーク対策として強制的にキャンセルをかける
			cancellationToken = GetActiveCancellationToken( cancellationToken ) ;

			//----------------------------------

			// 重要
			// 判定対象のタスクは実行中以外(失敗・中断・終了)は全て終了扱いとする
			// よって判定対象のタスクが中断された場合のオーナータスクを含めた一括中断は行われない

			int count ;

			while( true )
			{
				count = 0 ;

				foreach( var task in tasks )
				{
					if( task != null )
					{
						if( task is List<Task> )
						{
							// List<Task>
							List<Task> s_tasks = task as List<Task> ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null && s_task.Status == TaskStatus.Running )
								{
									count ++ ;	// 実行中
								}
							}
						}
						else
						if( task is List<UniTask> )
						{
							// List<UniTask>
							List<UniTask> s_tasks = task as List<UniTask> ;
							foreach( var s_task in s_tasks )
							{
								if( s_task.Status == UniTaskStatus.Pending )
								{
									count ++ ;	// 実行中(未実行は Succeeded)
								}
							}
						}
						else
						if( task is List<CustomYieldInstruction> )
						{
							// List<CustomYieldInstruction>
							List<CustomYieldInstruction> s_tasks = task as List<CustomYieldInstruction> ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null && s_task.keepWaiting == true )
								{
									count ++ ;	// 実行中
								}
							}
						}
						else
						if( task is Task[] )
						{
							// Task[]
							Task[] s_tasks = task as Task[] ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null && s_task.Status == TaskStatus.Running )
								{
									count ++ ;	// 実行中
								}
							}
						}
						else
						if( task is UniTask[] )
						{
							// UniTask[]
							UniTask[] s_tasks = task as UniTask[] ;
							foreach( var s_task in s_tasks )
							{
								if( s_task.Status == UniTaskStatus.Pending )
								{
									count ++ ;	// 実行中(未実行は Succeeded)
								}
							}
						}
						else
						if( task is CustomYieldInstruction[] )
						{
							// CustomYieldInstruction[]
							CustomYieldInstruction[] s_tasks = task as CustomYieldInstruction[] ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null && s_task.keepWaiting == true )
								{
									count ++ ;	// 実行中
								}
							}
						}
						else
						if( task is Task _1 )
						{
							// Task
							if( _1.Status == TaskStatus.Running )
							{
								count ++ ;	// 実行中
							}
						}
						else
						if( task is UniTask _2 )
						{
							// UniTask
							if( _2.Status == UniTaskStatus.Pending )
							{
								count ++ ;	// 実行中(未実行は Succeeded)
							}
						}
						else
						if( task is UniTask<bool>	_201 ){ if( _201.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<byte>	_202 ){ if( _202.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<char>	_203 ){ if( _203.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<short>	_204 ){ if( _204.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<ushort>	_205 ){ if( _205.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<int>	_206 ){ if( _206.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<uint>	_207 ){ if( _207.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<long>	_208 ){ if( _208.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<ulong>	_209 ){ if( _209.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<float>	_210 ){ if( _210.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<double>	_211 ){ if( _211.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<string>	_212 ){ if( _212.Status == UniTaskStatus.Pending ){ count ++ ; } }
						else
						if( task is CustomYieldInstruction _3 )
						{
							// CustomYieldInstruction
							if( _3.keepWaiting == true )
							{
								count ++ ;	// 実行中
							}
						}
						else
						if( task is Func<bool> _4 )
						{
							// Func<bool>
							if( _4() == false )
							{
								count ++ ;	// 実行中
							}
						}
					}
				}

				await UniTask.Yield( PlayerLoopTiming.Update, cancellationToken ) ;

				if( count == 0 )
				{
					// 全て終了
					break ;
				}
			}
		}

		/// <summary>
		/// 全ての非同期処理の終了を待つ
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public UniTask WhenAll( params Func<bool>[] funcs )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return WhenAll( default, funcs ) ;
		}

		/// <summary>
		/// 全ての非同期処理の終了を待つ
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public async UniTask WhenAll( CancellationToken cancellationToken, params Func<bool>[] funcs )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			// OnDestroy() 実行後の場合はリーク対策として強制的にキャンセルをかける
			cancellationToken = GetActiveCancellationToken( cancellationToken ) ;

			//----------------------------------

			// 重要
			// 判定対象のタスクは実行中以外(失敗・中断・終了)は全て終了扱いとする
			// よって判定対象のタスクが中断された場合のオーナータスクを含めた一括中断は行われない

			int count ;

			while( true )
			{
				count = 0 ;

				foreach( var func in funcs )
				{
					if( func != null )
					{
						if( func is Func<bool> _1 )
						{
							// Func<bool>
							if( _1() == false )
							{
								count ++ ;	// 条件が満たされていない
							}
						}
					}
				}

				await UniTask.Yield( PlayerLoopTiming.Update, cancellationToken ) ;

				if( count == 0 )
				{
					break ;
				}
			}
		}


		/// <summary>
		/// １つのタスクを待ちつつその結果を取得する
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public async UniTask<T> When<T>( UniTask<T> task, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			// OnDestroy() 実行後の場合はリーク対策として強制的にキャンセルをかける
			cancellationToken = GetActiveCancellationToken( cancellationToken ) ;

			//----------------------------------

			// 重要
			// 判定対象のタスクは実行中以外(失敗・中断・終了)は全て終了扱いとする
			// よって判定対象のタスクが中断された場合のオーナータスクを含めた一括中断は行われない

			while( true )
			{
				if( task.Status != UniTaskStatus.Pending )
				{
					break ;	// 終了
				}

				await UniTask.Yield( PlayerLoopTiming.Update, cancellationToken ) ;
			}

			if( task.Status == UniTaskStatus.Faulted )
			{
				// 失敗：例外扱いとする
				string errorMessage = $"[ExMonoBehaviour] When<T> : Task failed. in gameObject( {name} )" ;
				Debug.LogWarning( errorMessage ) ;
				throw new Exception( errorMessage ) ;
			}

			if( task.Status == UniTaskStatus.Canceled )
			{
				// 中断：デフォルト値が取得される(オーナータスクを含めた一括中断は行わない)
				return default ;
			}

			return task.GetAwaiter().GetResult() ;
		}

		/// <summary>
		/// １つのタスクコルーチンの終了を待つ
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public async UniTask When( System.Object task, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			// OnDestroy() 実行後の場合はリーク対策として強制的にキャンセルをかける
			cancellationToken = GetActiveCancellationToken( cancellationToken ) ;

			//----------------------------------

			// 重要
			// 判定対象のタスクは実行中以外(失敗・中断・終了)は全て終了扱いとする
			// よって判定対象のタスクが中断された場合のオーナータスクを含めた一括中断は行われない

			while( true )
			{
				if( task is Task _1 )
				{
					// Task
					if( _1.Status == TaskStatus.Faulted || _1.Status == TaskStatus.Canceled )
					{
						return ;
					}

					if( _1.Status != TaskStatus.Running )
					{
						return ;
					}
				}
				else
				if( task is UniTask _2 )
				{
					// UniTask
					if( _2.Status == UniTaskStatus.Faulted || _2.Status == UniTaskStatus.Canceled )
					{
						return ;
					}
					if( _2.Status == UniTaskStatus.Succeeded )
					{
						return ;
					}
				}
				else
				if( task is CustomYieldInstruction _3 )
				{
					// CustomYieldInstruction
					if( _3.keepWaiting != true )
					{
						// コルーチンが終了した(終了と非アクティブによる中断の区別はつかない)
						return ;
					}
				}
				else
				if( task is Func<bool> _4 )
				{
					// Func<bool>
					if( _4() != false )
					{
						return ;
					}
				}
				else
				{
					return ;
				}

				await UniTask.Yield( PlayerLoopTiming.Update, cancellationToken ) ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// ユーティリティ

		/// <summary>
		/// トゥイーンのヘルパー
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="easeType"></param>
		/// <returns></returns>
		protected async UniTask Tween( Action<float> onFrameUpdate, float duration, EaseTypes easeType = EaseTypes.Linear, float timeScale = 1 )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			if( onFrameUpdate == null )
			{
				return ;
			}

			if( duration <= 0 )
			{
				onFrameUpdate( 1 ) ;
				return ;
			}

			float timer = 0 ;
			while( timer <  duration )
			{
				timer += ( Time.deltaTime * timeScale ) ;
				if( timer >  duration )
				{
					timer  = duration ;
				}

				onFrameUpdate( Ease.GetValue( timer / duration, easeType ) ) ;

				await Yield() ;
			}
		}

		/// <summary>
		/// トゥイーンのヘルパー(動的なタイムスケール変更に対応)
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="easeType"></param>
		/// <returns></returns>
		protected async UniTask Tween( Func<float,float> onFrameUpdate, float duration, EaseTypes easeType = EaseTypes.Linear, float timeScale = 1 )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			if( onFrameUpdate == null )
			{
				return ;
			}

			if( duration <= 0 )
			{
				onFrameUpdate( 1 ) ;
				return ;
			}

			float timer = 0 ;
			while( timer <  duration )
			{
				timer += ( Time.deltaTime * timeScale ) ;
				if( timer >  duration )
				{
					timer  = duration ;
				}

				timeScale = onFrameUpdate( Ease.GetValue( timer / duration, easeType ) ) ;

				await Yield() ;
			}
		}

		/// <summary>
		/// トゥイーンのヘルパー(途中中断に対応)
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="easeType"></param>
		/// <returns></returns>
		protected async UniTask Tween( Func<float,bool> onFrameUpdate, float duration, EaseTypes easeType = EaseTypes.Linear, float timeScale = 1 )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			if( onFrameUpdate == null )
			{
				return ;
			}

			if( duration <= 0 )
			{
				return ;
			}

			float timer = 0 ;
			while( timer <  duration )
			{
				timer += ( Time.deltaTime * timeScale ) ;
				if( timer >  duration )
				{
					timer  = duration ;
				}

				if( onFrameUpdate( Ease.GetValue( timer / duration, easeType ) ) == true )
				{
					break ;
				}

				await Yield() ;
			}
		}

		/// <summary>
		/// トゥイーンのヘルパー(動的なタイムスケール変更と中断に対応)
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="easeType"></param>
		/// <returns></returns>
		protected async UniTask Tween( Func<float,( float, bool )> onFrameUpdate, float duration, EaseTypes easeType = EaseTypes.Linear, float timeScale = 1 )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			if( onFrameUpdate == null )
			{
				return ;
			}

			if( duration <= 0 )
			{
				onFrameUpdate( 1 ) ;
				return ;
			}

			float timer = 0 ;
			bool stop ;
			while( timer <  duration )
			{
				timer += ( Time.deltaTime * timeScale ) ;
				if( timer >  duration )
				{
					timer  = duration ;
				}

				( timeScale, stop ) = onFrameUpdate( Ease.GetValue( timer / duration, easeType ) ) ;

				if( stop == true )
				{
					break ;
				}

				await Yield() ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// 以下、タスク関連以外のメソッド群

		/// <summary>
		/// ゲームオブジェクトの複製を行う(親や姿勢は引き継ぐ)
		/// </summary>
		/// <returns></returns>
		public GameObject Duplicate()
		{
			GameObject clone = Instantiate( gameObject, transform.parent ) ;
			return clone ;
		}

		/// <summary>
		/// ゲームオブジェクトを複製し指定のコンポーネントを取得する(親や姿勢は引き継ぐ)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Duplicate<T>() where T : UnityEngine.Component
		{
			GameObject clone = Instantiate( gameObject, transform.parent ) ;
			return clone.GetComponent<T>() ;
		}

		/// <summary>
		/// 自身のゲームオブジェクト(インスタンス)を破棄する
		/// </summary>
		public void Destroy()
		{
#if UNITY_EDITOR
			if( Application.isPlaying == false )
			{
				DestroyImmediate( gameObject, false ) ;
			}
			else
#endif
			{
				Destroy( gameObject ) ;
			}
		}

		/// <summary>
		/// 他のゲームオブジェクト(インスタンス)を破棄する
		/// </summary>
		/// <param name="go"></param>
		protected void DestroyInstance( UnityEngine.Object instance )
		{
#if UNITY_EDITOR
			if( Application.isPlaying == false )
			{
				DestroyImmediate( instance, false ) ;
			}
			else
#endif
			{
				Destroy( instance ) ;
			}
		}
	}
}

