using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Reflection;

namespace Invoiced
{

    public abstract class AbstractEntity<T> where T : AbstractEntity<T> {

		private Connection Connection;
		private bool _entityCreated;
		protected string EndpointBase = "";
		protected string EntityName;

		// used to determine safe json serialisation. should always be null outside function bodies
		protected string CurrentOperation;

		public bool ShouldSerializeCurrentOperation() {
			return false;
		}

		protected string GetEndpointBase() {
			return EndpointBase;
		}

		public void SetEndpointBase(string endpointBase) {
			this.EndpointBase = endpointBase;
		}

		public string GetEndpoint(bool includeId)
		{
			String url = GetEndpointBase() + this.EntityName;

			if (this.EntityId() != null && includeId) {
				url += "/" + this.EntityId();
			}

			return url;
		}
		
		public override string ToString() {
			var s = base.ToString() + "<" + this.EntityId() +">";
			var jsonS =  s + " " + this.ToJsonString();

			return jsonS;
		}

		internal AbstractEntity(Connection conn) {
			this.Connection = conn;

		}

		protected AbstractEntity() {

		}

		protected Connection GetConnection() {
			return this.Connection;
		}

		public void ChangeConnection(Connection conn) {
			this.Connection = conn;
		}

		public virtual void Create() {

			if (this._entityCreated) {
				throw new EntityException("Object has already been created.");
			}

			if (!this.HasCrud()) {
				throw new EntityException("Create operation not supported on object.");
			}

			string url = this.GetEndpoint(false);
			string entityJsonBody = this.ToJsonString();
			string responseText = this.Connection.Post(url,null,entityJsonBody);
		
			try {
				JsonConvert.PopulateObject(responseText,this);
			} catch(Exception e) {
				throw new EntityException("",e);
			}

			this._entityCreated = true;

		}

		// this method serialises the existing object (with respect for defined create/update safety, i.e. ShouldSerialize functions)
		public virtual void SaveAll() {

			if (!this.HasCrud()) {
				throw new EntityException("Save operation not supported on object.");
			}

			string url = this.GetEndpoint(true);
			string entityJsonBody = this.ToJsonString();
			string responseText = this.Connection.Patch(url,entityJsonBody);
			
			try {
				JsonConvert.PopulateObject(responseText,this);
			} catch(Exception e) {
				throw new EntityException("",e);
			}


		}

		// this method does not serialise an existing object and therefore does not use defined create/update safety, i.e. ShouldSerialize functions)
		public void Save(string partialDataObject) {

			if (!this.HasCrud()) {
				throw new EntityException("Save operation not supported on object.");
			}

			string url = this.GetEndpoint(true);
			string responseText = this.Connection.Patch(url,partialDataObject);
			
			try {
				JsonConvert.PopulateObject(responseText,this);
			} catch(Exception e) {
				throw new EntityException("",e);
			}

		}

		public T Retrieve(long id)
		{
			return Retrieve(id.ToString());
		}

		public T Retrieve(string id = null)
		{

			string url = this.GetEndpoint(false);

			if (id != null) {
				url += "/" + id;
			}

			string responseText = this.Connection.Get(url,null);
			T serializedObject;
			try {
					serializedObject = JsonConvert.DeserializeObject<T>(responseText,new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
					serializedObject.Connection = this.Connection;
			} catch(Exception e) {
				throw new EntityException("",e);
			}

			return serializedObject;
			
		}

		public virtual void Delete() {

			if (!HasCrud()) {
				throw new EntityException("Delete operation not supported on object.");
			}

			string url = this.GetEndpoint(true);
			
			this.Connection.Delete(url);

		}

		private EntityList<T> List(string nextUrl,Dictionary<string,Object> queryParams, JsonConverter customConverter = null) {

			if (!this.HasList()) {
				throw new EntityException("List operation not supported on object.");
			}

			string url = this.GetEndpoint(false);
			
			if (!string.IsNullOrEmpty(nextUrl)) {
				url = nextUrl;
			}

			ListResponse response = this.Connection.GetList(url,queryParams);

			EntityList<T> entities;

			JsonSerializerSettings config = new JsonSerializerSettings {
				NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore
			};
			
			try {
				if (customConverter != null) {
					config.Converters.Add(customConverter);
				}
				entities = JsonConvert.DeserializeObject<EntityList<T>>(response.Result, config);
				
				entities.LinkURLS = response.Links;
				entities.TotalCount = response.TotalCount;
			} catch(Exception e) {
				throw new EntityException("",e);
			}

			foreach (var entity in entities) {
				entity.ChangeConnection(Connection);
			}

			return entities;

		}

		public EntityList<T> ListAll(Dictionary<string,Object> queryParams, JsonConverter customConverter = null) {
				var entities = ListAll("",queryParams, customConverter);
				return entities;
		}

		public EntityList<T> ListAll(string nextUrl = "",Dictionary<string,Object> queryParams = null, JsonConverter customConverter = null) {

			EntityList<T> entities = null;

			if (!this.HasList()) {
				throw new EntityException("List operation not supported on object.");
			}

			var tmpEntities = this.List(nextUrl,queryParams, customConverter);

			do {
				if (entities == null) {
					entities = tmpEntities;
				} else {
					entities.AddRange(tmpEntities);
					entities.LinkURLS = tmpEntities.LinkURLS;
					entities.TotalCount = tmpEntities.TotalCount;
				}

			} while((!string.IsNullOrEmpty(entities.GetNextURL()) && (entities.GetSelfURL() != entities.GetLastURL())));

			return entities;

		}

		protected string ToJsonString([System.Runtime.CompilerServices.CallerMemberName] string enclosingFunction = "") {
			if (enclosingFunction != "") {
				this.CurrentOperation = enclosingFunction;
			}

			var output = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented,new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore } );

			this.CurrentOperation = null;
			return output;
		}

		public void Void() {

			if (!this.HasVoid()) {
				throw new EntityException("Void operation not supported on object.");
			}

			string url = this.GetEndpoint(true) + "/void";

			string responseText = this.Connection.Post(url,null,null);
			
			try {
				JsonConvert.PopulateObject(responseText,this);
			} catch(Exception e) {
				throw new EntityException("",e);
			}
		}

		public IList<Attachment> ListAttachments() {

			if (!this.HasAttachments()) {
				throw new EntityException("List attachments operation not supported on object.");
			}

			IList<Attachment> objects = null;

			string url = this.GetEndpoint(true) + "/attachments";

			string responseText = this.Connection.Get(url,null);
			objects = JsonConvert.DeserializeObject<IList<Attachment>>(responseText,new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });

			return objects;
		}

		public IList<Email> SendEmail(EmailRequest emailRequest) {

			if (!this.HasSends()) {
				throw new EntityException("Send email operation not supported on object.");
			}

			IList<Email> objects = null;

			string url = this.GetEndpoint(true) + "/emails";

			string jsonRequestBody = emailRequest.ToJsonString();

			string responseText = this.Connection.Post(url,null,jsonRequestBody);
			objects = JsonConvert.DeserializeObject<IList<Email>>(responseText,new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });

			return objects;
		}

		public Letter SendLetter(LetterRequest letterRequest = null) {

			if (!this.HasSends()) {
				throw new EntityException("Send letter operation not supported on object.");
			}

			Letter letter = null;
			string responseText = null;

			string url = this.GetEndpoint(true) + "/letters";

			if (letterRequest != null) {
				string jsonRequestBody = letterRequest.ToJsonString();
				responseText = this.Connection.Post(url, null, jsonRequestBody);
			} else {
				responseText = this.Connection.Post(url, null, "");
			}

			letter = JsonConvert.DeserializeObject<Letter>(responseText,
				new JsonSerializerSettings
					{NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore});

				return letter;
		}

		public IList<TextMessage> SendText(TextRequest textRequest) {

			if (!this.HasSends()) {
				throw new EntityException("Send text message operation not supported on object.");
			}

			IList<TextMessage> objects = null;

			string url = this.GetEndpoint(true) + "/text_messages";

			string jsonRequestBody = textRequest.ToJsonString();

			string responseText = this.Connection.Post(url,null,jsonRequestBody);
			objects = JsonConvert.DeserializeObject<IList<TextMessage>>(responseText,new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });

			return objects;
		}

		protected abstract string EntityId();

		protected virtual bool HasCrud() {
			return true;
		}

		protected virtual bool HasList() {
			return true;
		}

		protected virtual bool HasVoid() {
			return false;
		}

		protected virtual bool HasAttachments() {
			return false;
		}

		protected virtual bool HasSends() {
			return false;
		}

    }

}
