namespace InstallHelper
{
    public interface IInstallManagementFeature
    {
        void InstallManagementFeature(string applicationFolder);

        void UninstallManagementFeature(string applicationFolder);
    }
}