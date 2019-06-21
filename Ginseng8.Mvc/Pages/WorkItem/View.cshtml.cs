using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.WorkItem
{
    public class ViewModel : DashboardPageModel
    {
        private readonly BlobStorage _blobStorage;

        public ViewModel(IConfiguration config) : base(config)
        {
            _blobStorage = new BlobStorage(config);
        }

        /// <summary>
        /// This is really WorkItem.Number
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public OpenWorkItemsResult Item { get; set; }
        public IEnumerable<Attachment> Attachments { get; set; }

        protected override void OnGetInternal(SqlConnection connection)
        {
            base.OnGetInternal(connection);

            Item = WorkItems?.First();
        }

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            Attachments = await new Attachments(QueryTraces) { OrgId = OrgId, ObjectType = ObjectType.WorkItem, ObjectId = Id, UserName = User.Identity.Name }.ExecuteAsync(connection);
        }

        protected override OpenWorkItems GetQuery()
        {
            return new OpenWorkItems(QueryTraces) { OrgId = OrgId, Number = Id, IsOpen = null };
        }

        public async Task<IActionResult> OnPostDeleteAttachmentAsync(int id)
        {
            using (var cn = Data.GetConnection())
            {
                var att = await cn.FindAsync<Attachment>(id);
                await _blobStorage.DeleteAsync(CurrentOrg.Name, att.Url);
                await cn.DeleteAsync<Attachment>(id);
            }

            return RedirectToPage("View");
        }

        public async Task<FileResult> OnGetDownloadAllAttachmentsAsync()
        {
            using (var cn = Data.GetConnection())
            {
                var attachments = await new Attachments() { OrgId = OrgId, ObjectType = ObjectType.WorkItem, ObjectId = Id, UserName = User.Identity.Name }.ExecuteAsync(cn);

                var container = await _blobStorage.GetOrgContainerAsync(Data.CurrentOrg.Name);

                using (var stream = new MemoryStream())
                {
                    using (var zip = new ZipArchive(stream, ZipArchiveMode.Create))
                    {
                        foreach (var att in attachments)
                        {
                            var uri = new Uri(att.Url);
                            var blob = new CloudBlockBlob(uri);
                            var entry = zip.CreateEntry(blob.Name);
                            using (var entryStream = entry.Open())
                            {
                                await blob.DownloadToStreamAsync(entryStream);
                            }
                        }
                    }
                    return File(stream.ToArray(), "application/zip", $"WorkItem-{Id}.zip");
                }
            }
        }
    }
}