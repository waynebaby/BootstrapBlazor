﻿using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BootstrapBlazor.Components
{
    /// <summary>
    /// JSRuntime 扩展操作类
    /// </summary>
    internal static class JSRuntimeExtensions
    {
        /// <summary>
        /// 弹出 Tooltip 组件
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="id"></param>
        /// <param name="method"></param>
        /// <param name="popoverType"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="html"></param>
        /// <param name="trigger"></param>
        public static async ValueTask Tooltip(this IJSRuntime jsRuntime, string? id, string method = "", PopoverType popoverType = PopoverType.Tooltip, string? title = "", string? content = "", bool html = false, string trigger = "hover focus")
        {
            if (!string.IsNullOrEmpty(id))
            {
                if (popoverType == PopoverType.Tooltip)
                    await jsRuntime.InvokeVoidAsync("$.bb_tooltip", id, method, title, html, trigger);
                else
                    await jsRuntime.InvokeVoidAsync("$.bb_popover", id, method, title, content, html, trigger);
            }
        }

        /// <summary>
        /// 调用 JSInvoke 方法
        /// </summary>
        /// <param name="jsRuntime">IJSRuntime 实例</param>
        /// <param name="el">Element 实例或者组件 Id</param>
        /// <param name="ref">DotNetObjectReference 实例</param>
        /// <param name="func">Javascript 方法</param>
        /// <param name="args">Javascript 参数</param>
        public static async ValueTask Invoke(this IJSRuntime jsRuntime, object? el = null, string? func = null, object? @ref = null, params object[] args)
        {
            var paras = new List<object>();
            if (el != null) paras.Add(el);
            if (@ref != null) paras.Add(@ref);
            if (args != null) paras.AddRange(args);
            await jsRuntime.InvokeVoidAsync($"$.{func}", paras.ToArray());
        }

        /// <summary>
        /// 调用 JSInvoke 方法
        /// </summary>
        /// <param name="jsRuntime">IJSRuntime 实例</param>
        /// <param name="el">Element 实例或者组件 Id</param>
        /// <param name="func">Javascript 方法</param>
        /// <param name="args">Javascript 参数</param>
        public static async ValueTask<TValue> InvokeAsync<TValue>(this IJSRuntime jsRuntime, object? el = null, string? func = null, params object[] args)
        {
            var paras = new List<object>();
            if (el != null) paras.Add(el);
            if (args != null) paras.AddRange(args);
            return await jsRuntime.InvokeAsync<TValue>($"$.{func}", paras.ToArray());
        }
    }
}
