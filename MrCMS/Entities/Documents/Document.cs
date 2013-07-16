﻿using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MrCMS.Entities.Documents.Web;
using MrCMS.Models;
using MrCMS.Paging;
using MrCMS.Services;
using MrCMS.Website;
using MrCMS.Helpers;
using NHibernate;

namespace MrCMS.Entities.Documents
{
    public abstract class Document : SiteEntity
    {
        [Required]
        [StringLength(255)]
        public virtual string Name { get; set; }

        public virtual Document Parent { get; set; }
        [Required]
        [DisplayName("Display Order")]
        public virtual int DisplayOrder { get; set; }

        public virtual string UrlSegment { get; set; }

        private IList<Document> _children = new List<Document>();
        private IList<Tag> _tags = new List<Tag>();

        public virtual IEnumerable<Webpage> PublishedChildren
        {
            get
            {
                return
                    Children.Select(webpage => webpage.Unproxy()).OfType<Webpage>().Where(document => document.Published)
                        .OrderBy(webpage => webpage.DisplayOrder);
            }
        }

        public virtual IList<Document> Children
        {
            get { return _children; }
            protected internal set { _children = value; }
        }

        public virtual IList<Tag> Tags
        {
            get { return _tags; }
            protected internal set { _tags = value; }
        }

        public virtual string TagList
        {
            get { return string.Join(", ", Tags.Select(x => x.Name)); }
        }

        public virtual int ParentId { get { return Parent == null ? 0 : Parent.Id; } }

        public virtual string DocumentType { get { return GetType().Name; } }

        /// <summary>
        /// Called before a document is to be deleted
        /// Place custom logic in here, or things that cannot be handled by NHibernate due to same table references
        /// </summary>
        public override void OnDeleting(ISession session)
        {
            if (Parent != null)
            {
                Parent.Children.Remove(this);
            }
            base.OnDeleting(session);
        }

        public virtual void OnSaving(ISession session)
        {

        }

        public virtual bool CanDelete
        {
            get { return !Children.Any(); }
        }

        protected internal virtual IList<DocumentVersion> Versions { get; set; }

        public virtual VersionsModel GetVersions(int page)
        {
            var documentVersions = Versions.OrderByDescending(version => version.CreatedOn).ToList();
            return
               new VersionsModel(
                   new PagedList<DocumentVersion>(
                       documentVersions, page, 10), Id);
        }

        protected internal virtual void CustomInitialization(IDocumentService service, ISession session) { }
    }
}