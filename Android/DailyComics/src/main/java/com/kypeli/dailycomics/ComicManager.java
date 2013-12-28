package com.kypeli.dailycomics;

import android.util.Log;

import com.kypeli.dailycomics.models.ComicListModel;
import com.kypeli.dailycomics.models.ComicModel;

import retrofit.RestAdapter;
import retrofit.http.GET;
import retrofit.http.Query;

import rx.Observable;
import rx.Observer;
import rx.Subscription;
import rx.concurrency.Schedulers;
import rx.subscriptions.Subscriptions;

/**
 * Created by user on 27/12/13.
 */
public class ComicManager {

    private final String TAG = "ComicManager";
    private final RestAdapter restAdapter = new RestAdapter.Builder()
                                            .setServer("http://scala-comic-server.herokuapp.com")
                                            .build();
    private final ComicManagerService comicService = restAdapter.create(ComicManagerService.class);

    private static ComicManager m_instance = null;

    public static ComicManager getInstance() {
        if (m_instance == null) {
            m_instance = new ComicManager();
        }

        return m_instance;
    }

    public Observable<ComicListModel.ComicListModelItem> getComicsObservable() {
        return Observable.create(new Observable.OnSubscribeFunc<ComicListModel.ComicListModelItem>() {

            @Override
            public Subscription onSubscribe(Observer<? super ComicListModel.ComicListModelItem> observer) {
                ComicListModel comics = comicService.getComics();
                for(ComicListModel.ComicListModelItem comic : comics.comics) {
                    observer.onNext(comic);
                }
                Log.d(TAG, "Got some comics");
                observer.onCompleted();

                return Subscriptions.empty();

            }
        }).subscribeOn(Schedulers.threadPoolForIO());
    }

    public Observable<ComicModel> getComicDetails(final String id) {
        return Observable.create(new Observable.OnSubscribeFunc<ComicModel>() {

            @Override
            public Subscription onSubscribe(Observer<? super ComicModel> observer) {
                ComicModel model = comicService.getComicDetails(id);
                observer.onNext(model);
                observer.onCompleted();

                return Subscriptions.empty();
            }

        }).subscribeOn(Schedulers.threadPoolForIO());
    }

    private interface ComicManagerService {
        @GET("/list")
        ComicListModel getComics();

        @GET("/comic")
        ComicModel getComicDetails(@Query("id") final String comicId);
    }
}
