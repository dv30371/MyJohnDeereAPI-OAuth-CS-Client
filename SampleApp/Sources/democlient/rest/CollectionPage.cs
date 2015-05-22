﻿using Hammock.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Sources.democlient.rest
{
    class CollectionPage<E> : List<E> {
        public int totalSize;
        public Uri nextPage;
        public Uri prevPage;
        public Uri self;
        public List<E> page;


    public CollectionPage(
            List<E> page,
            Uri self,
            Uri nextPage,
            Uri prevPage,
            int totalSize
                         ) {
        this.page = page;
        this.totalSize = totalSize;
        this.nextPage = nextPage;
        this.prevPage = prevPage;
        this.self = self;
    }


    }
}
