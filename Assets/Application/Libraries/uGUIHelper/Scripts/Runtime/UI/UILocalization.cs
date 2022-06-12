using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

using TMPro ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace uGUIHelper
{
	public class UILocalization
	{
		/// <summary>
		/// ローカライズ対応処理
		/// </summary>
		private static Func<string,string> m_OnProcess ;

		private static readonly Dictionary<Action<string>,string> m_Requests = new Dictionary<Action<string>, string>() ;


		public static bool AddRequest( Action<string> onLocalized, string key )
		{
			if( m_OnProcess != null )
			{
				// 既にローカライズ機構の準備が整っている
				onLocalized( m_OnProcess( key ) ) ;
				return true ;
			}

			// まだ準備が整っていないので一旦キューに貯める
			m_Requests.Add( onLocalized, key ) ;

			return false ;
		}

		public static void RemoveRequest( Action<string> onLocalized )
		{
			if( m_Requests.ContainsKey( onLocalized ) == true )
			{
				m_Requests.Remove( onLocalized ) ;
			}
		}

		/// <summary>
		/// ローカライズの処理機構をセットする
		/// </summary>
		/// <param name="onProcess"></param>
		public static void SetOnProcess( Func<string,string> onProcess )
		{
			m_OnProcess = onProcess ;
			if( m_OnProcess != null )
			{
				// 既にリクエストが溜まっていれば処理する
				if( m_Requests.Count >  0 )
				{
					foreach( var request in m_Requests )
					{
						request.Key( m_OnProcess( request.Value ) ) ;
					}
					m_Requests.Clear() ;
				}
			}
		}
	}
}
