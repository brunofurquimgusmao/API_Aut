#language: pt-br

Funcionalidade: API Personagem Star Wars
	Como consumidor da API
	Quero consultar um personagem especifico
	Para que eu possa visualizar os dados deste personagem

Contexto: 
	Dado que estou no endpoint 'https://swapi.dev/api/people/69/'

Cenário: Consultar um personagem utilizando ID
	#E informei o seguinte 'id' do personagem
	E utilizei o método do tipo 'GET'
	Quando chamar o serviço	
	Então o statuscode deverá ser 'OK'
	E uma resposta com a uma lista do tipo 'Automation_API.CustomerModel' deve ser retornada com os seguintes valores:
	| Name       | Height    | Mass	| HairColor | SkinColor | EyeColor | BirthYear | Gender | Homeworld | Films | Species | Vehicles | Starships | Created | Edited | Url |
	| Jango Fett |    183    |    79  |     black      |      tan     |    brown      |   66BBY        |     male   |     http://swapi.dev/api/planets/53/      |   http://swapi.dev/api/films/5/    |         |          |           |   2014-12-20T16:54:41.620000Z      |    2014-12-20T21:17:50.465000Z    |  http://swapi.dev/api/people/69/   |


