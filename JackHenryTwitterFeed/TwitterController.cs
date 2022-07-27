using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace JackHenryTwitterFeed
{
    public class TwitterController : Controller
    {
        [Route("twitter/gettrendinghashtags")]
        public ActionResult GetTrendingHashtags()
        {
            List<JackHenryTwitterService.TwitterExport> l = TwitterMemory._TwitterInboundService.GetTrendingHashtags(10);
            return Ok(l);
        }

        [Route("twitter/gettotaltweetsreceived")]
        public ActionResult GetTotalTweetsReceived()
        {
            int n = TwitterMemory._TwitterInboundService.GetTotalTweetsReceived();
            return Ok(Json(n));
        }
        [Route("twitter/getstoragecount")]
        public ActionResult GetStorageCount()
        {
            int n = TwitterMemory._TwitterInboundService.GetHashtagCount();
            return Ok(Json(n));
        }
        [Route("twitter/gethashtagswithlimit/{iLimit}")]
        public ActionResult GetHashtagsWithLimit(int iLimit = 10)
        {
            List<JackHenryTwitterService.TwitterExport> l = TwitterMemory._TwitterInboundService.GetTrendingHashtags(iLimit);
            var r = Ok(l);
            return r;
        }

    }
}
