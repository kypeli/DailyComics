package com.kypeli.dailycomics.models;

import java.util.List;

/**
 * Created by user on 27/12/13.
 */
public class ComicListModel {
    public List<ComicListModelItem> comics;

    public class ComicListModelItem {
        public String comicid;
        public String name;
    }
}
