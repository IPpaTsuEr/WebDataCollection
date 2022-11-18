using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDataCollection.Model.CollectionTask
{
    /// <summary>
    /// 任务执行结果类型
    /// </summary>
    public enum TaskResultType
    {
        /// <summary>
        /// 指向文件的Url数据，需要进行下载操作
        /// </summary>
        DownloadFile,

        /// <summary>
        /// 文本、Json字符串等数据
        /// </summary>
        SaveData,

        /// <summary>
        /// 新的任务Url地址
        /// </summary>
        TaskUrl,

        /// <summary>
        /// 在当前页面进行的其他操作
        /// </summary>
        //SubTaskUrl,

        /// <summary>
        /// 存入临时变量
        /// </summary>
        CacheData,

        /// <summary>
        /// 保存临时变量到文件
        /// </summary>
        SaveCacheData,

        /// <summary>
        /// 无数据（例如滚动页面、点击元素等）
        /// </summary>
        None,

        /// <summary>
        /// 循环执行子序列
        /// </summary>
        Loop,

        /// <summary>
        /// 等待(滚动、刷新页面后等待一段时间)
        /// </summary>
        Delay,

        /// <summary>
        /// 检查更新
        /// </summary>
        CheckUpdate

    }
}
