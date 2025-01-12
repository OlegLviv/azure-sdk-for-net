﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json;

namespace Azure.Security.KeyVault
{
    /// <summary>
    /// Defines a page in Azure responses.
    /// </summary>
    /// <typeparam name="T">Type of the page content items</typeparam>
    internal class KeyVaultPage<T> : IJsonDeserializable
        where T : IJsonDeserializable
    {
        private T[] _items;
        private readonly Func<T> _itemFactory;

        internal KeyVaultPage(Func<T> itemFactory)
        {
            _itemFactory = itemFactory;
        }

        /// <summary>
        /// Gets the content items.
        /// </summary>
        public ReadOnlySpan<T> Items { get => _items.AsSpan(); }

        /// <summary>
        /// Gets the link to the next page.
        /// </summary>
        public Uri NextLink { get; private set; }

        void IJsonDeserializable.ReadProperties(JsonElement json)
        {
            if (json.TryGetProperty("value", out JsonElement value))
            {
                if (value.ValueKind != JsonValueKind.Null)
                {
                    _items = new T[value.GetArrayLength()];

                    int i = 0;

                    foreach (JsonElement elem in value.EnumerateArray())
                    {
                        _items[i] = _itemFactory();

                        _items[i].ReadProperties(elem);

                        i++;
                    }
                }
            }

            if (json.TryGetProperty("nextLink", out JsonElement nextLink))
            {
                var nextLinkUrl = nextLink.GetString();

                if (!string.IsNullOrEmpty(nextLinkUrl))
                {
                    NextLink = new Uri(nextLinkUrl);
                }
            }
        }
    }
}
