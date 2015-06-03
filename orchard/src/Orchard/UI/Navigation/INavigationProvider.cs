namespace Orchard.UI.Navigation {
    /// <summary>
    /// 后台管理导航菜单
    /// </summary>
    public interface INavigationProvider : IDependency {
        string MenuName { get; }
        void GetNavigation(NavigationBuilder builder);
    }
}