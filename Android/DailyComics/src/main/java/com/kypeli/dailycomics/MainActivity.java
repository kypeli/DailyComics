package com.kypeli.dailycomics;

import java.util.ArrayList;
import java.util.List;

import android.app.Activity;
import android.app.Fragment;
import android.app.FragmentManager;
import android.support.v13.app.FragmentPagerAdapter;
import android.os.Bundle;
import android.support.v4.view.ViewPager;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.ScrollView;

import com.kypeli.dailycomics.models.ComicListModel;
import com.kypeli.dailycomics.models.ComicModel;

import rx.Observable;
import rx.android.concurrency.AndroidSchedulers;
import rx.concurrency.Schedulers;
import rx.util.functions.Action0;
import rx.util.functions.Action1;

public class MainActivity extends Activity {
    private final String TAG = "DC:MainActivity";
    private Observable<ComicListModel.ComicListModelItem> comicsObservable = ComicManager.getInstance().getComicsObservable();

    /**
     * The {@link android.support.v4.view.PagerAdapter} that will provide
     * fragments for each of the sections. We use a
     * {@link FragmentPagerAdapter} derivative, which will keep every
     * loaded fragment in memory. If this becomes too memory intensive, it
     * may be best to switch to a
     * {@link android.support.v13.app.FragmentStatePagerAdapter}.
     */
    SectionsPagerAdapter mSectionsPagerAdapter;

    /**
     * The {@link ViewPager} that will host the section contents.
     */
    ViewPager mViewPager;

    List<ComicListModel.ComicListModelItem> comics = new ArrayList<ComicListModel.ComicListModelItem>();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        // Create the adapter that will return a fragment for each of the three
        // primary sections of the activity.
        mSectionsPagerAdapter = new SectionsPagerAdapter(getFragmentManager());

        // Set up the ViewPager with the sections adapter.
        mViewPager = (ViewPager) findViewById(R.id.pager);
        mViewPager.setAdapter(mSectionsPagerAdapter);

        comicsObservable
                .subscribeOn(Schedulers.threadPoolForIO())
                .observeOn(AndroidSchedulers.mainThread())
                .subscribe(
                        // Next
                        new Action1<ComicListModel.ComicListModelItem>() {
                            @Override
                            public void call(ComicListModel.ComicListModelItem comicModel) {
                                Log.d(TAG, "We have a comic: " + comicModel.name);
                                comics.add(comicModel);
                            }

                        },
                        // Error
                        new Action1<Throwable>() {
                            @Override
                            public void call(Throwable throwable) {
                                Log.e(TAG, "Could not get all comics.");
                            }
                        },
                        // Completed
                        new Action0() {
                            @Override
                            public void call() {
                                mSectionsPagerAdapter.notifyDataSetChanged();
                            }
                        });

    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        switch (item.getItemId()) {
            case R.id.action_settings:
                return true;
        }
        return super.onOptionsItemSelected(item);
    }

    

    /**
     * A {@link FragmentPagerAdapter} that returns a fragment corresponding to
     * one of the sections/tabs/pages.
     */
    public class SectionsPagerAdapter extends FragmentPagerAdapter {

        public SectionsPagerAdapter(FragmentManager fm) {
            super(fm);
        }

        @Override
        public Fragment getItem(int position) {
            // getItem is called to instantiate the fragment for the given page.
            // Return a PlaceholderFragment (defined as a static inner class below).
            return PlaceholderFragment.newInstance(comics.get(position));
        }

        @Override
        public int getCount() {
            // Show 3 total pages.
            return comics.size();
        }

        @Override
        public CharSequence getPageTitle(int position) {
            return comics.get(position).name;
        }
    }

    /**
     * A placeholder fragment containing a simple view.
     */
    public static class PlaceholderFragment extends Fragment {
        /**
         * The fragment argument representing the section number for this
         * fragment.
         */
        private static final String ARG_COMICSTRIP_ID = "comicstrip_id";

        /**
         * Returns a new instance of this fragment for the given section
         * number.
         */
        public static PlaceholderFragment newInstance(ComicListModel.ComicListModelItem comicModel) {
            PlaceholderFragment fragment = new PlaceholderFragment();
            Bundle args = new Bundle();
            // args.putInt(ARG_SECTION_NUMBER, sectionNumber);
            args.putString(ARG_COMICSTRIP_ID, comicModel.comicid);
            fragment.setArguments(args);
            return fragment;
        }

        public PlaceholderFragment() {
        }

        @Override
        public View onCreateView(final LayoutInflater inflater, final ViewGroup container,
                Bundle savedInstanceState) {
            final String TAG = "DC:MainActivity:PlaceholderFragment";

            String id = getArguments().getString(ARG_COMICSTRIP_ID);
            final View comicView = new ComicView(getActivity().getBaseContext());
            final View rootView = inflater.inflate(R.layout.fragment_main, container, false);

            Observable<ComicModel> comicDetailsObservable = ComicManager.getInstance().getComicDetails(id);
            comicDetailsObservable
                    .subscribeOn(Schedulers.threadPoolForIO())
                    .observeOn(AndroidSchedulers.mainThread())
                    .subscribe(new Action1<ComicModel>() {
                        @Override
                        public void call(ComicModel comicModel) {
                            Log.d(TAG, "Ok, have details too: " + comicModel.name + " " + comicModel.url);

                            new ImageDownloader(comicView)
                                    .execute(comicModel.url);

                            final ScrollView scrollbar = (ScrollView)rootView.findViewById (R.id.scrollView);
                            scrollbar.addView(comicView);
                        }
                    });



         //   TextView textView = (TextView) rootView.findViewById(R.id.comic_image);
          //  textView.setText(Integer.toString(getArguments().getInt(ARG_SECTION_NUMBER)));
            return rootView;
        }
    }
}
