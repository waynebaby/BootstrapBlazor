﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BootstrapBlazor.Components
{
    /// <summary>
    /// Table 组件基类
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public abstract partial class TableBase<TItem> : BootstrapComponentBase, ITable where TItem : class, new()
    {
        /// <summary>
        /// 获得 wrapper 样式表集合
        /// </summary>
        protected string? WrapperClassName => CssBuilder.Default("table-wrapper")
            .AddClass("table-bordered", IsBordered)
            .Build();

        /// <summary>
        /// 获得 class 样式表集合
        /// </summary>
        protected string? ClassName => CssBuilder.Default("table")
            .AddClass("table-striped", IsStriped)
            .AddClass("table-hover", IsStriped)
            .AddClass("table-fixed", Height.HasValue)
            .AddClass("is-single", !IsMultipleSelect && ClickToSelect)
            .AddClassFromAttributes(AdditionalAttributes)
            .Build();

        /// <summary>
        /// 获得 Body 内行样式
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected string? GetRowClassString(TItem item) => CssBuilder.Default("")
           .AddClass(SetRowClassFormatter?.Invoke(item))
           .AddClass("active", CheckActive(item))
           .Build();

        /// <summary>
        /// 获得 表头 Model 实例
        /// </summary>
        protected TItem HeaderModel => Items?.FirstOrDefault() ?? new TItem();

        /// <summary>
        /// 获得/设置 可过滤表格列集合
        /// </summary>
        protected IEnumerable<ITableColumn>? FilterColumns { get; set; }

        /// <summary>
        /// 获得 表头集合
        /// </summary>
        public List<ITableColumn> Columns { get; } = new List<ITableColumn>(50);

        /// <summary>
        /// 获得/设置 TableHeader 实例
        /// </summary>
        [Parameter]
        public RenderFragment<TItem>? TableColumns { get; set; }

        /// <summary>
        /// 获得/设置 TableFooter 实例
        /// </summary>
        [Parameter]
        public RenderFragment? TableFooter { get; set; }

        /// <summary>
        /// 获得/设置 数据集合
        /// </summary>
        [Parameter]
        public IEnumerable<TItem>? Items { get; set; }

        /// <summary>
        /// 获得/设置 是否显示表脚 默认为 false
        /// </summary>
        [Parameter]
        public bool ShowFooter { get; set; }

        /// <summary>
        /// 获得/设置 是否斑马线样式
        /// </summary>
        [Parameter]
        public bool IsStriped { get; set; }

        /// <summary>
        /// 获得/设置 是否带边框样式
        /// </summary>
        [Parameter]
        public bool IsBordered { get; set; }

        /// <summary>
        /// 获得/设置 单击行回调委托方法
        /// </summary>
        [Parameter]
        public Func<TItem, Task>? OnClickRowCallback { get; set; }

        /// <summary>
        /// 获得/设置 双击行回调委托方法
        /// </summary>
        [Parameter]
        public Func<TItem, Task>? OnDoubleClickRowCallback { get; set; }

        /// <summary>
        /// OnInitialized 方法
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            // 初始化每页显示数量
            if (IsPagination)
            {
                PageItems = PageItemsSource.FirstOrDefault();

                if (Items != null) throw new InvalidOperationException($"Please set {nameof(OnQueryAsync)} instead set {nameof(Items)} property when {nameof(IsPagination)} be set True.");
            }

            // 初始化 EditModel
            if (EditModel == null)
            {
                if (OnAddAsync != null) EditModel = await OnAddAsync();
                else EditModel = new TItem();
            }

            // 设置 OnSort 回调方法
            OnSortAsync = QueryAsync;

            // 设置 OnFilter 回调方法
            OnFilterAsync = QueryAsync;

            // 如果未设置 Items 数据源 自动执行查询方法
            if (Items == null)
            {
                await QueryData();
                if (Items == null) Items = new TItem[0];
            }
        }

        #region 生成 Row 方法
        /// <summary>
        /// 获得 指定单元格数据方法
        /// </summary>
        /// <param name="col"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected RenderFragment GetValue(ITableColumn col, TItem item) => async builder =>
        {
            if (col.Template != null)
            {
                builder.AddContent(0, col.Template.Invoke(item));
            }
            else
            {
                string content = "";
                var val = GetItemValue(col.GetFieldName(), item);
                if (col.Formatter != null)
                {
                    // 格式化回调委托
                    content = await col.Formatter(val);
                }
                else if (!string.IsNullOrEmpty(col.FormatString))
                {
                    // 格式化字符串
                    content = val?.Format(col.FormatString) ?? "";
                }
                else if (col.FieldType.IsEnum())
                {
                    content = col.FieldType.ToDescriptionString(val?.ToString());
                }
                else
                {
                    content = val?.ToString() ?? "";
                }
                builder.AddContent(0, content);
            }
        };

        private object? GetItemValue(string fieldName, TItem item)
        {
            object? ret = null;
            if (item != null)
            {
                var invoker = GetPropertyCache.GetOrAdd((typeof(TItem), fieldName), key => item.GetPropertyValueLambda<TItem, object>(key.Item2).Compile());
                ret = invoker(item);
            }
            return ret;
        }

        private static readonly ConcurrentDictionary<(Type, string), Func<TItem, object>> GetPropertyCache = new ConcurrentDictionary<(Type, string), Func<TItem, object>>();
        #endregion
    }
}
