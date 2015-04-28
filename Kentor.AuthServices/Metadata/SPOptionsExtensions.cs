﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Kentor.AuthServices.Configuration;
using Kentor.AuthServices.WebSso;
using System.IdentityModel.Tokens;

namespace Kentor.AuthServices.Metadata
{
    static class SPOptionsExtensions
    {
        public static ExtendedEntityDescriptor CreateMetadata(this ISPOptions spOptions, AuthServicesUrls urls)
        {
            var ed = new ExtendedEntityDescriptor
            {
                EntityId = spOptions.EntityId,
                Organization = spOptions.Organization,
                CacheDuration = spOptions.MetadataCacheDuration
            };

            foreach (var contact in spOptions.Contacts)
            {
                ed.Contacts.Add(contact);
            }

            var spsso = new ExtendedServiceProviderSingleSignOnDescriptor();

            spsso.ProtocolsSupported.Add(new Uri("urn:oasis:names:tc:SAML:2.0:protocol"));

            spsso.AssertionConsumerServices.Add(0, new IndexedProtocolEndpoint()
            {
                Index = 0,
                IsDefault = true,
                Binding = Saml2Binding.HttpPostUri,
                Location = urls.AssertionConsumerServiceUrl
            });

            foreach(var attributeService in spOptions.AttributeConsumingServices)
            {
                spsso.AttributeConsumingServices.Add(attributeService);
            }


            if (spOptions.SigningCertificate != null)
            {
                using (var token = new X509SecurityToken(spOptions.SigningCertificate))
                {
                    var keyDescriptor = new KeyDescriptor(
                                            new SecurityKeyIdentifier(token.CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause>()));

                    spsso.Keys.Add(keyDescriptor);
                }
            }

            ed.RoleDescriptors.Add(spsso);

            if(spOptions.DiscoveryServiceUrl != null
                && !string.IsNullOrEmpty(spOptions.DiscoveryServiceUrl.OriginalString))
            {
                ed.Extensions.DiscoveryResponse = new IndexedProtocolEndpoint
                {
                    Binding = Saml2Binding.DiscoveryResponseUri,
                    Index = 0,
                    IsDefault = true,
                    Location = urls.SignInUrl
                };
            }

            return ed;
        }
    }
}
