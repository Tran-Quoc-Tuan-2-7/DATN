using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace Shopping_Tutorial.Repository
{
    public static class SessionExtensions
    {
        //Chuyển đổi object thành JSON bằng JsonConvert.SerializeObject(value).
        //Lưu trữ JSON vào session bằng session.SetString(key, jsonString)
        public static void SetJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        //Lấy dữ liệu JSON từ session bằng session.GetString(key).
        //Chuyển đổi JSON về object bằng JsonConvert.DeserializeObject<T>(sessionData).
        //Trả về default(T) nếu không có dữ liệu(ví dụ: null cho object, 0 cho số, false cho boolean).
        public static T GetJson<T>(this ISession session, string key)
        {
            var sessionData = session.GetString(key);
            return sessionData == null ? default(T) : JsonConvert.DeserializeObject<T>(sessionData);
        }
    }
}
