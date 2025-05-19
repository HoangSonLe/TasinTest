##Region Code Rules
- Private Params: Begin with "_" (_userId,_locationId,...)
- Public Params: Normal (userId, locationId,...)
- Use camel case for naming params'name (<EXAMPLE> : locationName)
- Always return type of API (Service,Controller): Acknowledgement, Acknowledgement<T>
- Controllers, Services,... must be inherit BaseClass (BaseController,BaseService,...)
- No magic params - meaning: convert all values to enums,settings,... (<Wrong>: i=> i.Type == "SomeValue" => <True>: i => i.Type == EnumType.SomeValue)
- Avoid double code => Write common functions
- 
- 
##EndRegion