using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Automation_API.Steps
{
    [Binding]
    public class API_Steps
    {

        [Given(@"que estou no endpoint '(.*)'")]
        public void DadoQueEstouNoEndpoint(string endpoint)
        {
            ScenarioContext.Current["Endpoint"] = endpoint;
        }
        
        [Given(@"informei o seguinte '(.*)' do personagem")]
        public void DadoInformeiOSeguinteDoPersonagem(string idpersonagem)
        {
            var endpoint = ScenarioContext.Current["Endpoint"];
            var args = ScenarioContext.Current[idpersonagem];

            if (endpoint.ToString().EndsWith(""))
                endpoint += $"/{args}";
            else
                endpoint += $"{args}";

            ScenarioContext.Current["Endpoint"] = endpoint;
        }
        
        [Given(@"utilizei o método do tipo '(.*)'")]
        public void DadoUtilizeiOMetodoDoTipo(string tipometodo)
        {
            var metodo = Method.POST;

            switch (tipometodo.ToUpper())
            {
                case "POST":
                    metodo = Method.POST;
                    break;
                case "GET":
                    metodo = Method.GET;
                    break;
                case "PUT":
                    metodo = Method.PUT;
                    break;
                case "DELETE":
                    metodo = Method.DELETE;
                    break;
                case "PATCH":
                    metodo = Method.PATCH;
                    break;
                default:
                    Assert.Fail("Método HTTP não esperado");
                    break;
            }

            ScenarioContext.Current["HttpMethod"] = metodo;
        }
        
        [When(@"chamar o serviço")]
        public void QuandoChamarOServico()
        {
            var endpoint = (String)ScenarioContext.Current["Endpoint"];

            ExecutarRequest(endpoint);
        }
        
        [Then(@"o statuscode deverá ser '(.*)'")]
        public void EntaoOStatuscodeDeveraSer(string statuscode)
        {
            var response = (IRestResponse)ScenarioContext.Current["Response"];

            string errorMessage;

            switch (response.StatusCode)
            {
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.NotFound:
                    errorMessage = "ResponseUri: " + response.ResponseUri;
                    break;
                case HttpStatusCode.Forbidden:
                    var auth = response.Request.Parameters.Where(x => x.Name == "Authorization").FirstOrDefault();
                    errorMessage = "Authorization: " + (auth != null ? auth.Value : "none");
                    break;
                default:
                    errorMessage = response.Content;
                    break;
            }

            Assert.AreEqual(statuscode, response.StatusCode.ToString(), errorMessage);
        }

        [Then(@"uma resposta com a uma lista do tipo '(.*)' deve ser retornada com os seguintes valores:")]
        public void EntaoUmaRespostaDeUmaListaXDeveSerRetornadaComOsSeguintesValores(string tipoModel, Table table)
        {
            var tipoModelType = Activator.CreateInstance("Automation_API", tipoModel).Unwrap();

            var listType = typeof(IEnumerable<>).MakeGenericType(tipoModelType.GetType());

            try
            {
                var response = (IRestResponse)ScenarioContext.Current["Response"];
                var content = response.Content;

                var model = JsonConvert.DeserializeObject(content, listType, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                });

                var reflValidarResposta = typeof(SetComparisonExtensionMethods).GetMethod("CompareToSet", System.Reflection.BindingFlags.Instance | BindingFlags.Static | System.Reflection.BindingFlags.Public).GetGenericMethodDefinition();

                var genericMethod = reflValidarResposta.MakeGenericMethod(tipoModelType.GetType());

                genericMethod.Invoke(null, new[] { table, model });
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                else
                    throw;
            }
        }


        #region Public

        public static IRestResponse ExecutarRequest(String endpoint)
        {
            var url = endpoint;

            var request = new RestRequest();

            request.Method = (Method)ScenarioContext.Current["HttpMethod"];

            request.Parameters.Clear();

            if (request.Method == Method.POST || request.Method == Method.PUT || request.Method == Method.PATCH)
            {
                var json = (String)ScenarioContext.Current["Data"];

                if (!String.IsNullOrWhiteSpace(json))
                    request.AddParameter("application/json", json, ParameterType.RequestBody);
            }


            var restClient = new RestClient(url);

            var response = restClient.Execute(request);

            ScenarioContext.Current["Response"] = response;

            return response;
        }

        public void ValidarResposta(Type typeToCompare, Table table)
        {
            var response = (IRestResponse)ScenarioContext.Current["Response"];
            var content = response.Content;

            var model = JsonConvert.DeserializeObject(content, typeToCompare, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
            });

            ScenarioContext.Current["ResponseModel"] = model;
            table.CompareToInstance(model);
        }

        #endregion
    }
}
