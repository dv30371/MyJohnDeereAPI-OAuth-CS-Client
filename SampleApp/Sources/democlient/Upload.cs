﻿using Hammock.Authentication.OAuth;
using Hammock.Web;
using Newtonsoft.Json;
using SampleApp.Sources.democlient.rest;
using SampleApp.Sources.generated.v3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Sources.democlient
{
    class Upload
    {
        private Dictionary<String, Link> links;
          private String userOrganizations;
           private Link fileUploadLink;
        private String newFileLocation;

        private Hammock.RestClient getRestClient()
        {
            Hammock.Authentication.OAuth.OAuthCredentials credentials = OAuthWorkFlow.createOAuthCredentials(OAuthType.ProtectedResource, ApiCredentials.TOKEN.token,
                ApiCredentials.TOKEN.secret, null, null);

            Hammock.RestClient client = new Hammock.RestClient()
            {
                Authority = "",
                Credentials = credentials
            };
            return client;

        }
        public void retrieveApiCatalog()  {
           Hammock.RestClient client =getRestClient();
            
            Hammock.RestRequest request = new Hammock.RestRequest()
            {
                Path = "https://apicert.soa-proxy.deere.com/platform/"
            };

            request.AddHeader("Accept", "application/vnd.deere.axiom.v3+json");
            Hammock.RestResponse response = client.Request(request);

            ApiCatalog apiCatalog = Download.Deserialise<ApiCatalog>(response.ContentStream);
 
            links = OAuthWorkFlow.linksFrom(apiCatalog);


            getCurrentUser();
            getUserOrganizations();
            addFile();
            uploadFile();
            deleteUploadedFile();
        }
        public void getCurrentUser() {
            Hammock.RestClient client =getRestClient();
            
            Hammock.RestRequest request = new Hammock.RestRequest()
            {
                Path = links["currentUser"].uri
            };

            request.AddHeader("Accept", "application/vnd.deere.axiom.v3+json");
            Hammock.RestResponse response = client.Request(request);

            Resource currentUser = Download.Deserialise<User>(response.ContentStream);

            userOrganizations = OAuthWorkFlow.linksFrom(currentUser)["organizations"].uri;

        }

        public void getUserOrganizations() {

            Hammock.RestClient client =getRestClient();
            
            Hammock.RestRequest request = new Hammock.RestRequest()
            {
                Path = userOrganizations
            };

            request.AddHeader("Accept", "application/vnd.deere.axiom.v3+json");
            Hammock.RestResponse response = client.Request(request);

            CollectionPageDeserializer ds = new CollectionPageDeserializer();

            CollectionPage<Organization> organizations = ds.deserialize<SampleApp.Sources.generated.v3.Organization>(response.Content);
            

            Dictionary<String,Link> linksFromFirst = OAuthWorkFlow.linksFrom(organizations.page[0]);

            fileUploadLink = linksFromFirst["uploadFile"];
    }
        public MemoryStream Serialize1<T>(T obj) {
            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));

            ser.WriteObject(stream1, obj);
            return stream1;

        }

        public void addFile() {

            SampleApp.Sources.generated.v3.File apiFile = new SampleApp.Sources.generated.v3.File();
            apiFile.name = "greatFileFromBrian.zip";

            Hammock.RestClient client =getRestClient();
            
            Hammock.RestRequest request = new Hammock.RestRequest()
            {
                Path = fileUploadLink.uri,
                Method = WebMethod.Post
            };

            request.AddHeader("Accept", "application/vnd.deere.axiom.v3+json");
            request.AddHeader("Content-Type", "application/vnd.deere.axiom.v3+json");

            String s = JsonConvert.SerializeObject(apiFile);

            request.AddPostContent(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(apiFile)));
            Hammock.RestResponse response = client.Request(request);

            newFileLocation = response.Headers["Location"];
    }


        public void uploadFile() {
            SampleApp.Sources.generated.v3.File apiFile = new SampleApp.Sources.generated.v3.File();
            apiFile.name = "greatFileFromBrian.zip";

            Hammock.RestClient client =getRestClient();
            
            Hammock.RestRequest request = new Hammock.RestRequest()
            {
                Path = fileUploadLink.uri,
                Method = WebMethod.Put
            };

            request.AddHeader("Accept", "application/vnd.deere.axiom.v3+json");
            request.AddHeader("Content-Type", "application/zip");
            MemoryStream ms = Serialize1<SampleApp.Sources.generated.v3.File>(apiFile);
            request.AddPostContent(ms.ToArray());
            Hammock.RestResponse response = client.Request(request);


        /*final RestRequest fileUploadRequest = oauthRequestTo(newFileLocation)
                .method("PUT")
                .addHeader(new HttpHeader("Accept", "application/vnd.deere.axiom.v3+json"))
                .addHeader(new HttpHeader("Content-Type", "application/zip"))
                .body(Files.newInputStreamSupplier(findFile("wdtTestFile.zip")))
                .build();

        final RestResponse fileUploadResponse = fileUploadRequest.fetchResponse();
        checkThat("PUT Response", fileUploadResponse.getResponseCode(), isEqualTo(NO_CONTENT));*/
    }

    public void deleteUploadedFile() {
        SampleApp.Sources.generated.v3.File apiFile = new SampleApp.Sources.generated.v3.File();
        Hammock.RestClient client = getRestClient();

        Hammock.RestRequest request = new Hammock.RestRequest()
        {
            Path = newFileLocation,
            Method = WebMethod.Delete
        };

        request.AddHeader("Accept", "application/vnd.deere.axiom.v3+json");
        Hammock.RestResponse response = client.Request(request);

      /*  final RestRequest deleteFileRequest = oauthRequestTo(newFileLocation)
                .method("DELETE")
                .addHeader(new HttpHeader("Accept", "application/vnd.deere.axiom.v3+json"))
                .build();

        final RestResponse deleteResponse = deleteFileRequest.fetchResponse();
        checkThat("DELETE Response", deleteResponse.getResponseCode(), isEqualTo(NO_CONTENT));*/
    }
    }
}
