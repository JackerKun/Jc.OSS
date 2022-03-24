# Jc.OSS

阿里云oss存储桶操作模块


#### 实例化
```c#
Jc.OSS.JcOSS oss = new Jc.OSS.JcOSS("bucketName","endPoint","accessKeyId","accessKeySecret");
```

#### 创建存储桶
```c#
CreateBucket(string bucketName)
```

#### 删除存储桶
```c#
DeleteBucket(string bucketName)
```

#### 同步上传一个文件
```c#
/// <summary>
/// 同步_上传一个新文件
/// </summary>
/// <param name="key">存储桶key</param>
/// <param name="localFile">本地文件路径</param>
/// <returns></returns>
bool PutObject(string key, string localFile)
```

#### 同步下载一个文件
```c#
/// <summary>
/// 同步_下载文件一个文件
/// </summary>
/// <param name="key">文件key</param>
/// <param name="localFilePath">本地保存文件的路径（全路径）</param>
/// <returns></returns>
bool GetObject(string key, string localFilePath)
```

#### 删除一个文件夹

```c#
/// <summary>
/// 删除文件
/// </summary>
/// <param name="key">文件Key</param>
/// <returns></returns>
bool DeleteObject(string key)
```

#### 判断文件是否存在
```c#
/// <summary>
/// 判断文件是否存在
/// </summary>
/// <param name="key"></param>
/// <returns></returns>
bool IsObject(string key)
```

#### 获取一个临时访问URL
```C#
/// <summary>
/// 获取临时访问Url
/// </summary>
/// <param name="key">文件Key</param>
/// <param name="Minutes">超时：分钟</param>
/// <returns></returns>
string GetObjectUrl(string key, int Minutes)
```

#### 异步上传文件
```c#
/// <summary>
/// 异步上传文件
/// </summary>
/// <param name="key">key</param>
/// <param name="localFile">本地文件路径</param>
/// <param name="contentType"></param>
/// <param name="callback">void callback(IAsyncResult ar)</param>
public void PutObjectAsync(string key, string localFile, string contentType, PutCallBack callback)

//上传回调
void PutCallback(System.Net.HttpStatusCode code)
{
    Console.WriteLine("put:"+code.ToString());
}
```

#### 异步下载文件
```c#
/// <summary>
/// 异步下载一个文件
/// </summary>
/// <param name="key"></param>
/// <param name="callback"></param>
/// <returns></returns>
public bool GetObjectAsync(string key, GetCallBack callback)

//下载回调
void GetCallBack(OssObject obj)
{
    if (obj.HttpStatusCode == HttpStatusCode.OK)
    {
        using (var fileStream = File.Create("local file path"))
        {
            obj.Content.CopyTo(fileStream);
        }
    }
}
```