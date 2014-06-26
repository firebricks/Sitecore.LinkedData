﻿using System;
using System.Linq;
using System.Security.Policy;
using LinkedData.Concepts;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Links;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace LinkedData.Helpers
{
    public static class SitecoreTripleHelper
    {
        public static Triple ToTriple(IConceptManager conceptManager, Item item, ItemLink itemLink)
        {
            var g = new Graph();

            g.NamespaceMap.AddNamespace("sitecore", new Uri("sitecore:"));
            //g.NamespaceMap.AddNamespace("http", new Uri("http:"));

            IUriNode sub = g.CreateUriNode(ItemToUri(item));
            ILiteralNode obj = g.CreateLiteralNode(ItemToUri(itemLink.GetTargetItem()));

            IUriNode predicate = conceptManager.GetPredicate(sub, obj);

            //Add Source Field Id to predicate
            IUriNode predicateWithFieldId = g.CreateUriNode(new Uri(predicate.Uri + "#" + itemLink.SourceFieldID));

            return new Triple(sub, Tools.CopyNode(predicateWithFieldId, g), obj);
        }

        public static string GetFieldIdFromPredicate(string uri)
        {
            var uriArray = uri.Split('#');

            if (uriArray.Count() > 1)
            {
                return uriArray[1].Replace("%7B", "{").Replace("%7D", "}");
            }

            return string.Empty;
        }

        public static string ItemToUri(Item item)
        {
            if (item != null && item.Uri != null)
            {
                return item.Uri.ToString().Replace("{", "%7B").Replace("}", "%7D");
            }
            return string.Empty;
        }

        public static Item UriToItem(string uri)
        {
            if (uri != null && !string.IsNullOrEmpty(uri) && uri.StartsWith("sitecore:"))
            {
                var itemUri = new ItemUri(uri.Replace("%7B", "{").Replace("%7D", "}"));

                var database = Sitecore.Configuration.Factory.GetDatabase(itemUri.DatabaseName);

                return database.GetItem(itemUri.ItemID, itemUri.Language, itemUri.Version);
            }
            return null;
        }

        public static SparqlQuery StringToSparqlQuery(string str)
        {
            var parser = new SparqlQueryParser();

            return parser.ParseFromString("PREFIX sitecore: <sitecore:>" + str);
        }
    }
}