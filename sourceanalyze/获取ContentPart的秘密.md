获取ContentPart时先根据contentType获取contentTypeDefinition

  var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            if (contentTypeDefinition == null) {
                contentTypeDefinition = new ContentTypeDefinitionBuilder().Named(contentType).Build();
            }

            // create a new kernel for the model instance
            var context = new ActivatingContentContext {
                ContentType = contentTypeDefinition.Name,
                Definition = contentTypeDefinition,
                Builder = new ContentItemBuilder(contentTypeDefinition)
            };

然后调用ContentHandler的Activating (invoke handlers to weld aspects onto kernel)

去装载部件，我们看ContentPartDriverCoordinator的Activating方法：
    public override void Activating(ActivatingContentContext context) {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null)
                return;

            var partInfos = _drivers.SelectMany(cpp => cpp.GetPartInfo()).ToList();

            foreach (var typePartDefinition in contentTypeDefinition.Parts) {
                var partName = typePartDefinition.PartDefinition.Name;
                var partInfo = partInfos.FirstOrDefault(pi => pi.PartName == partName);
                var part = partInfo != null 
                    ? partInfo.Factory(typePartDefinition) 
                    : new ContentPart { TypePartDefinition = typePartDefinition };
                context.Builder.Weld(part);
            }
        }

然后通过ContentHandler的Loading方法去加载Record
   // invoke handlers to acquire state, or at least establish lazy loading callbacks
            Handlers.Invoke(handler => handler.Loading(context), Logger);

        //先调用IContentHandler接口的Loading再调用具体实现类的Loading
		void IContentHandler.Loading(LoadContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Loading(context);
            //调用重载的Loading
            Loading(context);
        }            

Filters是在PartHandler构造函数传入的，比较特殊的如BodyPartRecord 因为它是继承自ContentPartVersionRecord
所以是加入的StorageVersionFilter。

		public static StorageFilter<TRecord> For<TRecord>(IRepository<TRecord> repository) where TRecord : ContentPartRecord, new() {		
     		//如果TRecord是ContentPartVersionRecord的子类那么返回StorageVersionFilter
            if (typeof(TRecord).IsSubclassOf(typeof(ContentPartVersionRecord))) {
                var filterType = typeof(StorageVersionFilter<>).MakeGenericType(typeof(TRecord));
                return (StorageFilter<TRecord>)Activator.CreateInstance(filterType, repository);
            }
            return new StorageFilter<TRecord>(repository);
        }
我们再来看看StorateFilter的Loading方法，其实是加入Loader，然后在获取Record的时候再去调用Loader中的委托。
		protected override void Loading(LoadContentContext context, ContentPart<TRecord> instance) {
            var versionRecord = context.ContentItemVersionRecord;
            instance._record.Loader(prior => GetRecordCore(versionRecord) ?? CreateRecordCore(versionRecord, prior));
        }