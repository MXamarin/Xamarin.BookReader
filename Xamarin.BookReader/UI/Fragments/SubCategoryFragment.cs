﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.BookReader.Bases;
using Xamarin.BookReader.Models;
using DSoft.Messaging;
using Xamarin.BookReader.Models.Support;
using Xamarin.BookReader.UI.Activities;
using Xamarin.BookReader.UI.EasyAdapters;
using Xamarin.BookReader.Datas;
using System.Reactive.Concurrency;
using Xamarin.BookReader.Utils;
using System.Reactive.Linq;

namespace Xamarin.BookReader.UI.Fragments
{
    /// <summary>
    /// 二级分类
    /// </summary>
    [Register("xamarin.bookreader.ui.fragments.SubCategoryFragment")]
    public class SubCategoryFragment : BaseRVFragment<BooksByCats.BooksBean>
    {
        public static String BUNDLE_MAJOR = "major";
        public static String BUNDLE_MINOR = "minor";
        public static String BUNDLE_GENDER = "gender";
        public static String BUNDLE_TYPE = "type";

        public static SubCategoryFragment newInstance(String major, String minor, String gender,
                                                      String type)
        {
            SubCategoryFragment fragment = new SubCategoryFragment();
            Bundle bundle = new Bundle();
            bundle.PutString(BUNDLE_MAJOR, major);
            bundle.PutString(BUNDLE_GENDER, gender);
            bundle.PutString(BUNDLE_MINOR, minor);
            bundle.PutString(BUNDLE_TYPE, type);
            fragment.Arguments = (bundle);
            return fragment;
        }


        private String major = "";
        private String minor = "";
        private String gender = "";
        private String type = "";

        public override int LayoutResId => Resource.Layout.common_easy_recyclerview;

        public override void InitDatas()
        {
            MessageBus.Default.Register<SubEvent>(initCategoryList);
            major = Arguments.GetString(BUNDLE_MAJOR);
            gender = Arguments.GetString(BUNDLE_GENDER);
            minor = Arguments.GetString(BUNDLE_MINOR);
            type = Arguments.GetString(BUNDLE_TYPE);
        }

        public override void ConfigViews()
        {
            initAdapter(new SubCategoryAdapter(Activity), true, true);
            onRefresh();
        }
        public void showCategoryList(BooksByCats data, bool isRefresh)
        {
            if (isRefresh)
            {
                start = 0;
                mAdapter.clear();
            }
            mAdapter.addAll(data.books);
            start += data.books.Count();
        }
        public void showError()
        {
            loaddingError();
        }
        public void complete()
        {
            mRecyclerView.setRefreshing(false);
        }
        public void initCategoryList(object sender, MessageBusEvent evnt)
        {
            var e = evnt as SubEvent;
            Activity.RunOnUiThread(() =>
            {
                minor = e.minor.ToString();
                if (type.Equals(e.type.ToString()))
                {
                    onRefresh();
                }
            });
        }
        public override void onItemClick(int position)
        {
            BooksByCats.BooksBean data = mAdapter.getItem(position);
            BookDetailActivity.startActivity(Activity, data._id);
        }
        public override void onRefresh()
        {
            base.onRefresh();
            getCategoryList(gender, major, minor, this.type, 0, limit);
        }
        public override void onLoadMore()
        {
            getCategoryList(gender, major, minor, this.type, start, limit);
        }
        void getCategoryList(String gender, String major, String minor, String type, int start, int limit)
        {
            BookApi.Instance.getBooksByCats(gender, type, major, minor, start, limit)
                .SubscribeOn(DefaultScheduler.Instance)
                .ObserveOn(Application.SynchronizationContext)
                .Subscribe(data => {
                    bool isRefresh = start == 0 ? true : false;
                    showCategoryList(data, isRefresh);
                }, e => {
                    LogUtils.e("GirlBookDiscussionFragment", e.ToString());
                    showError();
                }, () => {
                    LogUtils.i("GirlBookDiscussionFragment", "complete");
                    complete();
                });
        }

        public override void OnDestroyView()
        {
            MessageBus.Default.DeRegister<SubEvent>(initCategoryList);
            base.OnDestroyView();
        }
    }
}