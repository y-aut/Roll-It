using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Menu Sceneで開いているページやタブの情報を管理
public struct MenuHistory
{
    public MenuPage Page;
    public StageViewTabs StageViewTab;

    public MenuHistory(MenuPage page)
    {
        Page = page;
        StageViewTab = StageViewTabs.Trending;
    }

    public MenuHistory(StageViewTabs tab)
    {
        Page = MenuPage.Find;
        StageViewTab = tab;
    }
}
