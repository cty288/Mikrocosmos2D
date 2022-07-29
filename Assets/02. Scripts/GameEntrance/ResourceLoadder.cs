using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Managers;
using MikroFramework.ResKit;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mikrocosmos
{
    public class ResourceLoadder : HotUpdateEntranceManager, IController
    {
        private IGameResourceModel gameResourceModel;
        private void Awake() {
            gameResourceModel = this.GetModel<IGameResourceModel>();
        }

        protected override void LaunchInDevelopingMode() {
            StartHotUpdate();
        }

        protected override void LaunchInTestingMode() {
            StartHotUpdate();
        }


        protected override void OnHotUpdateError(HotUpdateError error) {
            Debug.Log(error.ToString());
        }

        protected override void OnHotUpdateManagerInitialized()
        {

        }

        protected override void OnHotUpdateVersionChecked(bool needUpdate, ResVersion localResVersion)
        {
            if (needUpdate) {
                Debug.Log("Need update");
            }
            else
            {
                Debug.Log("Do not need update");
            }
        }

        public TMP_Text downloadText;
        protected override void OnHotUpdateResourcesDownloading(float downloadProgress, float totalDownloadSize, float alreadyDownloadedFileSize,
            float downloadingFileDownloadedSize, float downloadSpeed)
        {
            downloadText.text = $"Total downloaded progress: {HotUpdateManager.Singleton.Downloader.GetDownloadProgress()}" +
                                $"\n Total downloading size: {HotUpdateManager.Singleton.Downloader.GetTotalDownloadFileSize()}" +
                                $"\n Already downloaded size: {HotUpdateManager.Singleton.Downloader.GetAlreadyDownloadedFileSize()}" +
                                $"\n Downloading file size: {HotUpdateManager.Singleton.Downloader.GetDownloadingFileSize()}" +
                                $"\n Download speed (kb/s): {HotUpdateManager.Singleton.Downloader.GetDownloadSpeed()}";
        }

        protected override void OnHotUpdateResourceDownloadedAndUpdated()
        {

        }

        protected override void OnResourceCompletenessValidated(List<ABMD5Base> updatedResourceInfos) {
          
        }

        protected override void OnRedundantFilesDeleted()
        {

        }

        protected override void OnHotUpdateComplete() {
            gameResourceModel.LoadNecessaryResources((() => {
                this.GetModel<IGoodsConfigurationModel>().LoadGoodsProperties((() => {
                    SceneManager.LoadScene("Menu");
                }));
            }));
        }

        public IArchitecture GetArchitecture() {
            return Mikrocosmos.Interface;
        }
    }
}
