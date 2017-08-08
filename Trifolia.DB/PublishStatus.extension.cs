using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public partial class PublishStatus
    {
        public const string PUBLISHED_STATUS = "Published";
        public const string DRAFT_STATUS = "Draft";
        public const string TEST_STATUS = "Test";
        public const string BALLOT_STATUS = "Ballot";
        public const string DEPRECATED_STATUS = "Deprecated";
        public const string RETIRED_STATUS = "Retired";

        #region Public Methods

        /// <summary>
        /// Gets whether this <see cref="PublishStatus"/> instance has a status of "published"
        /// </summary>
        /// <returns></returns>
        public bool IsPublished
        {
            get
            {
                return this.Status == "Published";
            }
        }

        public bool IsBallot
        {
            get
            {
                return this.Status == BALLOT_STATUS;
            }
        }

        public bool IsDraft
        {
            get
            {
                return this.Status == DRAFT_STATUS;
            }
        }
        public bool IsTest
        {
            get
            {
                return this.Status == TEST_STATUS;
            }
        }
        public bool IsDeprecated
        {
            get
            {
                return this.Status == DEPRECATED_STATUS;
            }
        }

        public static PublishStatus GetPublishedStatus(IObjectRepository tdb)
        {
            return tdb.PublishStatuses.Single(y => y.Status == PUBLISHED_STATUS);
        }

        public static PublishStatus GetDraftStatus(IObjectRepository tdb)
        {
            return tdb.PublishStatuses.Single(y => y.Status == DRAFT_STATUS);
        }

        public static PublishStatus GetTestStatus(IObjectRepository tdb)
        {
            return tdb.PublishStatuses.Single(y => y.Status == TEST_STATUS);
        }

        public static PublishStatus GetDeprecatedStatus(IObjectRepository tdb)
        {
            return tdb.PublishStatuses.Single(y => y.Status == DEPRECATED_STATUS);
        }

        public static PublishStatus GetRetiredStatus(IObjectRepository tdb)
        {
            return tdb.PublishStatuses.Single(y => y.Status == RETIRED_STATUS);
        }

        public static PublishStatus GetBallotStatus(IObjectRepository tdb)
        {
            return tdb.PublishStatuses.Single(y => y.Status == BALLOT_STATUS);
        }

        #endregion
    }
}
