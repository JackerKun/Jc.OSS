using System;
using System.IO;
using System.Threading;
using Aliyun.OSS;
using Aliyun.OSS.Common;

namespace Jc.OSS
{
    public class JcOSS
    {
        /// <summary>
        /// 开发者秘钥对，通过阿里云控制台的秘钥管理页面创建与管理
        /// </summary>
        private readonly string _accessKeyId = null;

        /// <summary>
        /// 开发者秘钥对，通过阿里云控制台的秘钥管理页面创建与管理
        /// </summary>
        private readonly string _accessKeySecret = null;

        /// <summary>
        /// 存储桶
        /// </summary>
        private readonly string _bucketName = null;

        /// <summary>
        /// 节点
        /// </summary>
        private readonly string _endpoint = null;

        /// <summary>
        /// 桶实例
        /// </summary>
        public readonly OssClient _client;

        /// <summary>
        /// oss 访问类
        /// </summary>
        /// <param name="bucketName">存储筒名字</param>
        /// <param name="endPoint">节点</param>
        /// <param name="accessKeyId">key</param>
        /// <param name="accessKeySecret">secret</param>
        public JcOSS(string bucketName, string endPoint, string accessKeyId, string accessKeySecret)
        {
            _bucketName = bucketName;
            _endpoint = endPoint;
            _accessKeySecret = accessKeySecret;
            _accessKeyId = accessKeyId;
            _client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);
        }


        /// <summary>
        /// 创建一个新的存储空间
        /// </summary>
        /// <param name="bucketName">存储桶名字</param>
        /// <returns></returns>
        public Bucket CreateBucket(string bucketName)
        {
            Bucket result = null;
            try
            {
                result = _client.CreateBucket(bucketName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 删除存储桶
        /// </summary>
        /// <param name="bucketName">存储桶名字</param>
        /// <returns></returns>
        public bool DeleteBucket(string bucketName)
        {
            bool result = false;
            try
            {
                _client.DeleteBucket(bucketName);
                result = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 同步_上传一个新文件
        /// </summary>
        /// <param name="key">存储桶key</param>
        /// <param name="localFile">本地文件路径</param>
        /// <returns></returns>
        public bool PutObject(string key, string localFile)
        {
            bool res = false;
            try
            {
                key = key.Replace(@"\", "/");
                PutObjectResult result = _client.PutObject(_bucketName, key, localFile);
                if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    res = true;
                }
                else
                {
                    res = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }

        /// <summary>
        /// 追加上传文件
        /// </summary>
        /// <param name="key">存储桶key</param>
        /// <param name="localFile">本地文件路径</param>
        /// <returns></returns>
        public bool AppendObject(string key, string localFile)
        {
            bool res = false;
            // 第一次追加的位置是0，返回值为下一次追加的位置。后续追加的位置是追加前文件的长度。
            long position = 0;
            try
            {
                var metadata = _client.GetObjectMetadata(_bucketName, key);
                position = metadata.ContentLength;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            try
            {
                // 获取追加位置，再次追加文件。
                using (var fs = File.Open(localFile, FileMode.Open))
                {
                    var request = new AppendObjectRequest(_bucketName, key);
                    //{
                    //    ObjectMetadata = new ObjectMetadata(),
                    //    Content = fs,
                    //    Position = position
                    //};
                    var result = _client.AppendObject(request);
                    position = result.NextAppendPosition;
                    Console.WriteLine("Append object succeeded, next append position:{0}", position);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }

        /// <summary>
        /// 获取临时访问Url
        /// </summary>
        /// <param name="key">文件Key</param>
        /// <param name="Minutes">超时：分钟</param>
        /// <returns></returns>
        public string GetObjectUrl(string key, int Minutes)
        {
            string result = null;
            try
            {
                key = key.Replace(@"\", "/");
                if (key.StartsWith(@"/"))
                {
                    key = key.Substring(1, key.Length - 1);
                }
                //先判断文件是否存在
                if (IsObject(key))
                {
                    // 生成下载签名URL。
                    var req = new GeneratePresignedUriRequest(_bucketName, key, SignHttpMethod.Get)
                    {
                        Expiration = DateTime.Now.AddMinutes(Minutes),
                    };

                    Uri uri = _client.GeneratePresignedUri(req);
                    result = uri.AbsoluteUri;
                }
                else
                {
                    result = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 同步_下载文件一个文件
        /// </summary>
        /// <param name="key">文件key</param>
        /// <param name="localFilePath">本地保存文件的路径（全路径）</param>
        /// <returns></returns>
        public bool GetObject(string key, string localFilePath)
        {
            bool res = false;
            try
            {
                key = key.Replace(@"\", "/");
                OssObject result = _client.GetObject(_bucketName, key);
                res = StreamToFile(result.Content, localFilePath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }

        /// <summary>
        /// Stream保存到文件
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool StreamToFile(Stream stream, string fileName)
        {
            bool result = false;
            try
            {
                using (var requestStream = stream)
                {
                    byte[] buf = new byte[1024];
                    var fs = File.Open(fileName, FileMode.OpenOrCreate);
                    var len = 0;
                    // 通过输入流将文件的内容读取到文件或者内存中。
                    while ((len = requestStream.Read(buf, 0, 1024)) != 0)
                    {
                        fs.Write(buf, 0, len);
                    }
                    fs.Close();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="key">文件Key</param>
        /// <returns></returns>
        public bool DeleteObject(string key)
        {
            bool result = false;
            try
            {
                _client.DeleteObject(_bucketName, key);
                result = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsObject(string key)
        {
            bool result = false;
            try
            {
                result = _client.DoesObjectExist(_bucketName, key);
            }
            catch (OssException ex)
            {
                result = false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 列出存储空间内的所有文件
        /// </summary>
        public ObjectListing ListObjects()
        {
            ObjectListing result = null;
            try
            {
                string nextMarker = string.Empty;
                do
                {
                    ListObjectsRequest listObjectsRequest = new ListObjectsRequest(_bucketName)
                    {
                        Marker = nextMarker,
                        MaxKeys = 100
                    };
                    result = _client.ListObjects(listObjectsRequest);
                    nextMarker = result.NextMarker;
                } while (result.IsTruncated);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 异步上传回调 委托
        /// </summary>
        public delegate void PutCallBack(System.Net.HttpStatusCode status);

        /// <summary>
        /// 异步下载回调 委托
        /// </summary>
        public delegate void GetCallBack(OssObject obj);

        /// <summary>
        /// 上传回调声明
        /// </summary>
        private event PutCallBack PutCallback;

        /// <summary>
        /// 下载回调声明
        /// </summary>
        private event GetCallBack GetCallback;

        private void PutObjectCallback(IAsyncResult ar)
        {
            try
            {
                PutObjectResult res = _client.EndPutObject(ar);
                PutCallback(res.HttpStatusCode);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                 _event.Set();
            }
        }

        private void GetObjectCallback(IAsyncResult ar)
        {
            try
            {
                OssObject res = _client.EndGetObject(ar);
                GetCallback(res);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                 _event.Set();
            }
        }

         private AutoResetEvent _event = new AutoResetEvent(false);

        /// <summary>
        /// 异步上传文件
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="localFile">本地文件路径</param>
        /// <param name="contentType"></param>
        /// <param name="callback">void callback(IAsyncResult ar)</param>
        public void PutObjectAsync(string key, string localFile, string contentType, PutCallBack callback)
        {
            try
            {
                PutCallback = callback;
                using (var fs = File.Open(localFile, FileMode.Open))
                {
                    key = key.Replace(@"\", "/");
                    var metadata = new ObjectMetadata();
                    // 增加自定义元信息。
                    //metadata.UserMetadata.Add("Value", "Key");
                    metadata.CacheControl = "No-Cache";
                    metadata.ContentType = contentType; //provider.Mappings[Path.GetExtension(_localFile)];
                    string result = "upload success";
                    // 异步上传。
                    IAsyncResult resultObj = _client.BeginPutObject(_bucketName, key, fs, metadata, PutObjectCallback, result.ToCharArray());
                    _event.WaitOne();
                }
            }
            catch (OssException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 异步下载一个文件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool GetObjectAsync(string key, GetCallBack callback)
        {
            bool res = false;
            try
            {
                GetCallback = callback;
                key = key.Replace(@"\", "/");
                string result = "upload success";
                IAsyncResult resultObj =
                    _client.BeginGetObject(_bucketName, key, GetObjectCallback, result.ToCharArray());
                _event.WaitOne();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }
        
    }
}