using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine ;

using Template.Enumerator ;

using static Template.PlayerData ;

//<auto-generated>
//Beast.ShareClientGeneratorによる自動生成されたファイルです。手動変更禁止
//<auto-generated>
namespace Template.WebAPIs
{
    [Serializable]
    public partial class ResponseBase
    {
        [SerializeField]
        public ResponseCodes responseCode ;
        public ResponseCodes ResponseCode { get { return responseCode ; } }

        [SerializeField]
        public String   responseMessage ;
        public String   ResponseMessage { get { return responseMessage ; } }
        
        [SerializeField]
        public long     date ;
        public long     Date { get { return date ; } }

        //-----------------------------

        [SerializeField]
        public Updater  updater ;
        public Updater  Updater { get { return updater ; } }

    }
}