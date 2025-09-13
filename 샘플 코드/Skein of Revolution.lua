--@ BeginProperty
--@ SyncDirection=None
number stage = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
number setPositionX = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
number setPositionY = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
number mapCount = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
boolean generated = "false"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
table values = "{}"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
string value = """"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
table mapIDs = "{}"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
table corridorIDs = "{}"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
number beforeMapID = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
number currentMapID = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
string currentMapValue = ""entry""
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
table state = "{}"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
string image = """"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
Entity spawnedMap = "nil"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
table spawnedMaps = "{}"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
table passedMaps = "{}"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
string destination = """"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
number totalMapCount = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
number totalCorridorCount = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
number corridorRB = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
table corridorRBs = "{}"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
Entity entryMap = "nil"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
number currentPosX = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
number currentPosY = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
Entity locationMark = "nil"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
table ranBatMark = "{}"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
string curDes = """"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
number oldMapCount = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
boolean oldMapItem = "false"
--@ EndProperty

--@ BeginMethod
--@ MethodExecSpace=All
void TotalCount()
{
local dataSetNum = "MapPositionData" .. math.floor(self.stage)
local positionDataSet = _DataService:GetTable(dataSetNum)

local randomMapDataSet = _DataService:GetTable("RandomMapData")

self.totalMapCount = tonumber(randomMapDataSet:GetCell(self.stage,"total"))
--log(self.totalMapCount)

local corridorDataSetNum = "CorridorPosition" .. math.floor(self.stage)
local corridorPositionDataSet = _DataService:GetTable(corridorDataSetNum)

self.totalCorridorCount = tonumber(corridorPositionDataSet:GetRowCount())
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void SetCorridorInfo()
{
local count = math.ceil(self.totalCorridorCount / 2)

if self.stage == 1 then
	for i = 1, count+1, 1 do
		self:CorridorPosition(i)
		self:Hor_CorridorSpawn(i)
		self.corridorRBs[i] = false
	end
	
	for j = count+2, self.totalCorridorCount, 1 do
		self:CorridorPosition(j)
		self:Col_CorridorSpawn(j)
		self.corridorRBs[j] = false
	end
end

if self.stage >=2 and self.stage < 5 then
	for i = 1, count, 1 do
		self:CorridorPosition(i)
		self:Hor_CorridorSpawn(i)
		self.corridorRBs[i] = false
	end
	
	for j = count+1, self.totalCorridorCount, 1 do
		self:CorridorPosition(j)
		self:Col_CorridorSpawn(j)
		self.corridorRBs[j] = false
	end
end

if self.stage == 5 then
	for i = 1, self.totalCorridorCount, 1 do
		self:CorridorPosition(i)
		self:Hor_CorridorSpawn(i)
		self.corridorRBs[i] = false
	end
end

--self:RandomBattleCorridor()

--[[
for i = 1, self.totalCorridorCount, 1 do
	if self.corridorRBs[i] == true then
		log('corriodorRB-',i,self.corridorRBs[i])
	end
end
]]--
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=ClientOnly
table CorridorPosition(number i)
{
local dataSetNum = "CorridorPosition" .. math.floor(self.stage)
local positionDataSet = _DataService:GetTable(dataSetNum)

self.setPositionX = tonumber(positionDataSet:GetCell(i,1))
self.setPositionY = tonumber(positionDataSet:GetCell(i,2))
----log(self.state)

return {self.setPositionX, self.setPositionY}
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=ClientOnly
void Col_CorridorSpawn(number i)
{
local parent = _EntityService:GetEntityByPath("/ui/MapGroup/UISprite")
local mapVector = Vector3(self.setPositionX, self.setPositionY, 0)

local spawnedCorridor = _SpawnService:SpawnByModelId("model://f65b06e9-8060-4167-b528-3dd7c66bdf55", "Col_Corridor", mapVector, parent)
spawnedCorridor.UITransformComponent.anchoredPosition.x = self.setPositionX
spawnedCorridor.UITransformComponent.anchoredPosition.y = self.setPositionY
self.corridorIDs[i] = spawnedCorridor

----log(mapVector)
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=ClientOnly
void Hor_CorridorSpawn(number i)
{
local parent = _EntityService:GetEntityByPath("/ui/MapGroup/UISprite")
local mapVector = Vector3(self.setPositionX, self.setPositionY, 0)

local spawnedCorridor = _SpawnService:SpawnByModelId("model://1ed681f1-cd93-44af-bd6f-8b81bf6f9e4f", "Hor_Corridor", mapVector, parent)
spawnedCorridor.UITransformComponent.anchoredPosition.x = self.setPositionX
spawnedCorridor.UITransformComponent.anchoredPosition.y = self.setPositionY
self.corridorIDs[i] = spawnedCorridor

----log(mapVector)
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void SetRandomBattleCorridor()
{
if self.stage == 1 or self.stage == 2 or self.stage == 5 then
	return
end

if self.stage == 3 then
	self.corridorRB = 2
end
	
if self.stage == 4 then
	self.corridorRB = 4
end

local randomNum = math.random(1, self.totalCorridorCount)

while self.corridorRB ~= 0 do
	if self.corridorRBs[randomNum] == true then
		randomNum = math.random(1, self.totalCorridorCount)
	end
	
	if self.corridorRBs[randomNum] == false then
		self.corridorRBs[randomNum] = true
		self.corridorRB = self.corridorRB - 1
	end
end

self:RandomBattleCorridor()
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=ClientOnly
void RandomBattleCorridor()
{
--self:PassedCorridor()
for i = 1, self.totalCorridorCount, 1 do
	if self.corridorRBs[i] == true then
		local corridorDataSetNum = "CorridorPosition" .. math.floor(self.stage)
		local corridorPositionDataSet = _DataService:GetTable(corridorDataSetNum)
		local corridorPosX = corridorPositionDataSet:GetCell(i, 'x')
		local corridorPosY = corridorPositionDataSet:GetCell(i, 'y')
		
		local parent = _EntityService:GetEntityByPath("/ui/MapGroup/UISprite")
		local mapVector = Vector3(self.setPositionX, self.setPositionY, 0)
		
		local ranBattle = _SpawnService:SpawnByModelId("model://ff42208b-ecbc-4a17-857c-1f6fcc7486ef", 'RandomBattle', mapVector, parent)
		ranBattle.UITransformComponent.Position.x = tonumber(corridorPosX)
		ranBattle.UITransformComponent.Position.y = tonumber(corridorPosY)
		
		self.ranBatMark[i] = ranBattle
	end
end
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void PassedCorridor()
{
--log('hi')
local dataSetNum = "CorridorPosition" .. math.floor(self.stage)
local positionDataSet = _DataService:GetTable(dataSetNum)

for i = 1, self.totalCorridorCount, 1 do
	local link = positionDataSet:GetCell(i, 'link')
	local splitedLink = string.gmatch(link, '[^,]+')
	
	for j = 1, self.totalMapCount, 1 do
		local beforeCount = 0
		local currentCount = 0
		
		for linkMap in splitedLink do
			linkMap = tonumber(linkMap)
			
			if linkMap == self.beforeMapID then
				beforeCount = beforeCount + 1
				--log('before count', beforeCount)
				--log(self.beforeMapID)
			end
			
			if linkMap == self.currentMapID then
				currentCount = currentCount + 1
				--log('current count', currentCount)
			end
		end
		
		if beforeCount == 1 and currentCount == 1 then
			self.corridorRBs[i] = true
			log('spawnCorridor: ',i)
		end
	end
end
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void GoRandomBattle()
{
if self.stage < 5 and self.stage > 1 then

	local dataSetNum = "MapPositionData" .. math.floor(self.stage)
	local dataSet = _DataService:GetTable(dataSetNum)
	local before = dataSet:GetCell(self.beforeMapID, "linkC")
	--local beforeLinkC = string.gmatch(before, '[^,]+')
	local beforeLinkC = Regex(","):Split(before)
	local current = dataSet:GetCell(self.currentMapID, "linkC")
	--local currentLinkC = string.gmatch(current, '[^,]+')
	local currentLinkC = Regex(","):Split(current)
	
	local player = _UserService.LocalPlayer
	
	log('beforeID'..self.beforeMapID.. 'currentID'.. self.currentMapID)
	for i = 1, self.totalCorridorCount, 1 do
		log(i, self.corridorRBs[i])
	end
	log('before = ' .. before .. ', current = ' .. current)
	for i, findBC in pairs(beforeLinkC) do
		findBC = tonumber(findBC)
		--log('findBC = ' .. findBC)
		
		for j, findCC in pairs(currentLinkC) do
			findCC = tonumber(findCC)
			--log('findCC = ' .. findCC)
			
			if findBC == findCC and self.corridorRBs[findBC] == true then
				log(self.beforeMapID..'의'..tostring(findBC) .. '와' ..self.currentMapID..'의'.. tostring(findCC) .. '를 검사.')
				log(findBC..'의 true? ' .. tostring(self.corridorRBs[findBC]))
				
				if self.oldMapCount == 0 and self.oldMapItem == false then
					self:OldMapboard()
				end
				
				if self.oldMapCount > 0 then
					self.oldMapCount = self.oldMapCount - 1
					log('oldMapCount ='.. self.oldMapCount)
					return
				end
				
				
				player.BattleTurnManager.isRandom = true
				self.curDes = self.destination
				self.destination = "/maps/BattleMap/SpawnLocation"
			end
		end
	end

end
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void SetMapInfo()
{
local mapDataSet = _DataService:GetTable("MapVariety")

self.value = mapDataSet:GetCell(5, 1)
self:EntryPosition()
self:MapSpawn()
self.mapIDs[self.mapCount] = math.floor(self.mapCount)
self.values[self.mapCount] = self.value

self.value = mapDataSet:GetCell(6, 1)
self:RewardPosition()
self:MapSpawn()
self.mapIDs[self.mapCount] = math.floor(self.mapCount)
self.values[self.mapCount] = self.value

if self.stage == 1 then
	for i = 1, 4, 1 do
		self.value = mapDataSet:GetCell(i, 1)
		self:FirstStageMap(i)
		self:MapSpawn()
	end
end

if self.stage >= 2 then
	for i = 1, 4, 1 do
		----log(i)
		self.value = mapDataSet:GetCell(i, 1)
		----log(self.value)
		self:Count()
	end
end
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=ClientOnly
void ShowMap()
{
local mapUIEntity = _EntityService:GetEntityByPath("/ui/MapGroup/UISprite")
mapUIEntity:SetEnable(true) 
local tweenfx = function(tweenValue)
	mapUIEntity.UITransformComponent.Position.y = tweenValue
end
local mapopen = _TweenLogic:MakeTween(-1000, 0, 0.75, EaseType.BackEaseOut, tweenfx)
mapopen.AutoDestroy = true
mapopen:Play()

--self:CurrentMap()
self:ReactivateMap()
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void FirstStageMap(number valueNum)
{
local posNum = 0

if valueNum == 1 then
	posNum = 5
end

if valueNum == 2 then
	posNum = 6
end

if valueNum == 3 then
	posNum = 1
end

if valueNum == 4 then
	posNum = 2
end

local dataSetNum = "MapPositionData" .. math.floor(self.stage)
local positionDataSet = _DataService:GetTable(dataSetNum)

self.setPositionX = tonumber(positionDataSet:GetCell(posNum,1))
self.setPositionY = tonumber(positionDataSet:GetCell(posNum,2))

self.mapIDs[posNum] = posNum
self.values[posNum] = self.value
self.mapCount = posNum
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void EntryPosition()
{
local dataSetNum = "MapPositionData" .. math.floor(self.stage)
local positionDataSet = _DataService:GetTable(dataSetNum)
local entryPos = 0

if self.stage == 1 then
	entryPos = 4
end

if self.stage == 2 then
	entryPos = 7
end

if self.stage == 3 then
	entryPos = 13
end

if self.stage == 4 then
	entryPos = 16
end

if self.stage == 5 then
	entryPos = 1
end

self.beforeMapID = entryPos
self.setPositionX = tonumber(positionDataSet:GetCell(entryPos,1))
self.setPositionY = tonumber(positionDataSet:GetCell(entryPos,2))
self.state[entryPos] = true
self.mapCount = entryPos
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void RewardPosition()
{
local dataSetNum = "MapPositionData" .. math.floor(self.stage)
local positionDataSet = _DataService:GetTable(dataSetNum)
local rewardPos = 0

if self.stage == 1 then
	rewardPos = 3
end

if self.stage == 2 then
	rewardPos = 3
end

if self.stage == 3 then
	rewardPos = 4
end

if self.stage == 4 then
	rewardPos = 5
end

if self.stage == 5 then
	rewardPos = 2
end

self.setPositionX = tonumber(positionDataSet:GetCell(rewardPos,1))
self.setPositionY = tonumber(positionDataSet:GetCell(rewardPos,2))
self.state[rewardPos] = true
self.mapCount = rewardPos
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=ClientOnly
table MapPosition()
{
local dataSetNum = "MapPositionData" .. math.floor(self.stage)
local positionDataSet = _DataService:GetTable(dataSetNum)

local randomNum = math.random(1,self.totalMapCount)

for i = 1, self.totalMapCount, 1 do
	--log(self.state[i])
end

while self.state[randomNum] == true do
	randomNum = math.random(1,self.totalMapCount)
end
	
	----log(self.state)
self.setPositionX = tonumber(positionDataSet:GetCell(randomNum,1))
self.setPositionY = tonumber(positionDataSet:GetCell(randomNum,2))
self.state[randomNum] = true
self.mapCount = randomNum
	
self.mapIDs[self.mapCount] = math.floor(self.mapCount)

--[[
if self.stage == 5 and self.value == 'battle' then
	self.values[self.mapCount] = 'boss'
else self.values[self.mapCount] = self.value end
]]--

----log(self.state)

return {self.setPositionX, self.setPositionY}
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=ClientOnly
void MapImage()
{
if self.value == "battle" then
	self.image = "183d7765a12d4d9384dbc508da540838"
end

if self.value == "entry" then
	self.image = "2faf6e58ee1f4ea78b4b9822c266a45b"
end

if self.value == "reward" then
	self.image = "e1ebf8e87ac246a09d793a7d0254c599"
end

if self.value == "puzzle" then
	self.image = "ca9ee337d9214fcfb264812ada595699"
end

if self.value == "event" then
	self.image = "a64c00c15dcb460ca2482ac5309aeceb"
end

if self.value == "store" then
	self.image = "a5bc5773829e44d4871c2add695d5fbd"
end

if self.value == 'boss' then
	self.image = "b656cc80dd164afa8c1129524db1a824"
end
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=ClientOnly
void Count()
{
local randomDataSet = _DataService:GetTable("RandomMapData")
local totalVariety = tonumber(randomDataSet:GetCell(self.stage, self.value))


for j = 1, totalVariety, 1 do
	--self.mapCount = self.mapCount + 1
	self:MapPosition()
	self:MapSpawn()
end
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=ClientOnly
void MapSpawn()
{
local parent = _EntityService:GetEntityByPath("/ui/MapGroup/UISprite")
local mapVector = Vector3(self.setPositionX, self.setPositionY, 0)

local spawnedMap = _SpawnService:SpawnByModelId("model://48ffa2a1-ef6f-4c95-a247-06ad0a0602bf", "MapFoothold", mapVector, parent)
spawnedMap.UITransformComponent.anchoredPosition.x = self.setPositionX
spawnedMap.UITransformComponent.anchoredPosition.y = self.setPositionY

if self.stage == 5 and self.value == 'battle' then
	self.value = 'boss'
	spawnedMap.SpawnedMapInfo.value = 'boss'
end

self:MapImage()
spawnedMap.SpriteGUIRendererComponent.ImageRUID.DataId = self.image
spawnedMap.SpawnedMapInfo.mapID = math.floor(self.mapCount)
spawnedMap.SpawnedMapInfo.value = self.value
spawnedMap.SpawnedMapInfo.spawnedMap = spawnedMap
self.mapIDs[self.mapCount] = self.mapCount
self.spawnedMaps[self.mapCount] = spawnedMap
--log(self.spawnedMaps[self.mapCount])

if self.value == 'entry' then
	self.entryMap = spawnedMap
	if self.currentMapID == self.entryMap.SpawnedMapInfo.mapID then
	self.currentMapID = self.mapCount
	self.currentPosX = self.setPositionX
	self.currentPosY = self.setPositionY
	--log('entryMap:',self.currentPosX, self.currentPosY)
	end
end
----log(spawnedMap.SpawnedMapInfo.mapID, spawnedMap.SpawnedMapInfo.value)

local eventRandomNum = math.random(1,11)
spawnedMap.SpawnedMapInfo.scenarioNum = eventRandomNum

self.spawnedMap = spawnedMap


--self.spawnedMap.ButtonComponent.Enable = true
self:ActivateMap(spawnedMap.SpawnedMapInfo.mapID, spawnedMap)


--[[
if self.mapCount == 16 or self.mapCount == 11 or self.mapCount == 17 then
	spawnedMap.ButtonComponent.Enable = true
	spawnedMap.self:Highlight()
end 
]]--
log("MapSpawn")
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=ClientOnly
void GetValue()
{
local spawnMapValue = self.spawnedMap.SpawnedMapInfo.value

if self.spawnedMap.SpawnedMapInfo.empty == true then
	self.destination = "/maps/EmptyMap/SpawnLocation"
end

if self.spawnedMap.SpawnedMapInfo.empty == false then
	
	self:FavoriteDoll()
	
	if spawnMapValue == "battle" then
		self.destination = "/maps/BattleMap/SpawnLocation"
	end
		
	if spawnMapValue == "puzzle" then
		self.destination = "/maps/PuzzleMap/SpawnLocation"
	end
		
	if spawnMapValue== "entry" then
		self.destination = "/maps/EntryMap/SpawnLocation"
	end
		
	if spawnMapValue == "reward" then
		self.destination = "/maps/RewardMap/SpawnLocation"
	end
		
	if spawnMapValue == "event" then
		self.destination = "/maps/EventMap/SpawnLocation"
	end
		
	if spawnMapValue == "store" then
		self.destination = "/maps/StoreMap/SpawnLocation"
	end
	
	if spawnMapValue == "boss" then
		self.destination = "/maps/BossMap/SpawnLocation"
	end
	
end


}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=ClientOnly
void ActivateMap(number mapID,Entity spawnedMap)
{
local dataSetNum = "MapPositionData" .. math.floor(self.stage)
local positionDataSet = _DataService:GetTable(dataSetNum)
--log('success! currentMapID:', tostring(self.currentMapID))

if self.currentMapID == 0 then
	if self.stage == 1 then
		self.currentMapID = 4
	end
		
	if self.stage == 2 then
		self.currentMapID = 7
	end
		
	if self.stage == 3 then
		self.currentMapID = 13
	end
		
	if self.stage == 4 then
		self.currentMapID = 16
	end
	
	if self.stage == 5 then
		self.currentMapID = 1
	end
end

local link = positionDataSet:GetCell(self.currentMapID, 'link')
local splitedLink = string.gmatch(link, '[^,]+')

local currentMapID = math.floor(self.currentMapID)

if mapID == self.currentMapID then
	spawnedMap.ButtonComponent.Enable = true
	--spawnedMap.SpriteGUIRendererComponent.ImageRUID = 
	
	if spawnedMap.SpawnedMapInfo.tweener ~= nil then
	spawnedMap.SpawnedMapInfo.tweener:Destroy()
	spawnedMap.UITransformComponent.UIScale = Vector3(1,1,1)
	end
end

for checkMapID in splitedLink do
	checkMapID = tonumber(checkMapID)
	if mapID == checkMapID then
		spawnedMap.ButtonComponent.Enable = true
		self:Highlight(true,spawnedMap)
	end
end

self:PassedMap()
--self:RandomBattleCorridor()
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=ClientOnly
void ReactivateMap()
{
for i = 1, self.totalMapCount, 1 do
	local mapID = self.mapIDs[i]
	local spawnedMap = self.spawnedMaps[i]
	--spawnedMap.ButtonComponent.Colors.DisabledColor = Color(1,1,1,1)
	self:ActivateMap(mapID,spawnedMap)
end
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void UnactivateMap()
{
for i = 1, self.totalMapCount, 1 do
	local spawnedMap = self.spawnedMaps[i]
	spawnedMap.ButtonComponent.Enable = false
	if spawnedMap.SpawnedMapInfo.tweener ~= nil then
	spawnedMap.SpawnedMapInfo.tweener:Destroy()
	spawnedMap.UITransformComponent.UIScale = Vector3(1,1,1)
	end
end
_EntityService:Destroy(self.locationMark)

for i = 1, self.totalCorridorCount, 1 do
	local ranBatMark = self.ranBatMark[i]
	_EntityService:Destroy(ranBatMark)
end
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void Highlight(boolean generated,Entity spawnedMap)
{
if generated == true then
local tween = _TweenLogic:ScaleTo(spawnedMap, Vector2(1.05,1.05), 0.5, EaseType.QuadEaseInOut)
tween.LoopCount = -1
tween.LoopType = TweenLoopType.PingPong
spawnedMap.SpawnedMapInfo.tweener = tween
end
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void PassedMap()
{
--[[
for i = 1, 20, 1 do
	--log('passedMaps-', self.passedMaps[i])
end
]]--
for i = 1, self.totalMapCount, 1 do
	if self.passedMaps[i] ~= nil then
	local passedMap = self.passedMaps[i]
	passedMap.SpriteGUIRendererComponent.Color = Color(0.7,0.7,0.7,1)
	passedMap.ButtonComponent.Colors.DisabledColor = Color(0.9,0.9,0.9,1)
	passedMap.SpawnedMapInfo.empty = true
	end
end
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void CurrentMap()
{
local parent = _EntityService:GetEntityByPath("/ui/MapGroup/UISprite")
local mapVector = Vector3(self.setPositionX, self.setPositionY, 0)

local currentMap = _SpawnService:SpawnByModelId("model://b748e802-e8a1-411e-b41c-5d9c461aa656", 'LocationMark', mapVector, parent)
self.locationMark = currentMap
currentMap.UITransformComponent.Position.x = self.currentPosX
currentMap.UITransformComponent.Position.y = self.currentPosY + 20
--log('현재:',self.currentPosX, self.currentPosY)

local tween = _TweenLogic:MoveTo(currentMap, Vector2(currentMap.UITransformComponent.Position.x, currentMap.UITransformComponent.Position.y+20), 0.5, EaseType.QuadEaseInOut)
tween.LoopCount = -1
tween.LoopType = TweenLoopType.PingPong
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void EntryMap()
{
self.entryMap.SpriteGUIRendererComponent.ImageRUID = 'b656cc80dd164afa8c1129524db1a824'
self.entryMap.SpawnedMapInfo.value = 'boss'
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void MapBackNTile(Entity tileRUID,Entity backgroundRUID)
{
local emVal = self.currentMapValue
local tile = tileRUID.TileMapComponent.TileSetRUID
local background = backgroundRUID.BackgroundComponent.TemplateRUID

if self.stage == 1 then
	if emVal == "entry" then
		return
	end
	
	if emVal == "battle" then
		tile = "6dcaf05b7f204c239a3c08fb1bff2f7f"
		background = "0ce08ea829ed49259d2814929acb5527"
	end
	
	if emVal == "puzzle" then
		tile = "6dcaf05b7f204c239a3c08fb1bff2f7f"
		background = "0ce08ea829ed49259d2814929acb5527" 
	end
	
	if emVal == "event" then
		tile = "6dcaf05b7f204c239a3c08fb1bff2f7f"
		background = "0ce08ea829ed49259d2814929acb5527" 
	end
	
	if emVal == "store" then
		tile = "6dcaf05b7f204c239a3c08fb1bff2f7f"
		background = "0ce08ea829ed49259d2814929acb5527" 
	end
		
	if emVal == "reward" then
		tile = "099eb7d77d184c34908fcef37918462e"
		background = "29537f30f5c045f4864d73f0c562fcd3"
	end
end

if self.stage == 2 then
	if emVal == "entry" then
		return
	end
	
	if emVal == "battle" then
		tile = "46701ff2021b4d1fb21fbf5790b1ab14"
		background = "5598590147e94c6eb41a791d355d22f0"
	end
	
	if emVal == "puzzle" then
		tile = "46701ff2021b4d1fb21fbf5790b1ab14"
		background = "5598590147e94c6eb41a791d355d22f0" 
	end
	
	if emVal == "event" then
		tile = "46701ff2021b4d1fb21fbf5790b1ab14"
		background = "5598590147e94c6eb41a791d355d22f0" 
	end
		
	if emVal == "store" then
		tile = "46701ff2021b4d1fb21fbf5790b1ab14"
		background = "5598590147e94c6eb41a791d355d22f0" 
	end
		
	if emVal == "reward" then
		tile = "099eb7d77d184c34908fcef37918462e"
		background = "29537f30f5c045f4864d73f0c562fcd3"
	end
end

if self.stage == 3 then
	if emVal == "entry" then
		return
	end
	
	if emVal == "battle" then
		tile = "33a77f587f2a4018b79cae30d286db9d"
		background = "72f8b2d517384f67a32c39d889e2738e"
	end
	
	if emVal == "puzzle" then
		tile = "33a77f587f2a4018b79cae30d286db9d"
		background = "72f8b2d517384f67a32c39d889e2738e" 
	end
	
	if emVal == "event" then
		tile = "33a77f587f2a4018b79cae30d286db9d"
		background = "72f8b2d517384f67a32c39d889e2738e" 
	end
	
	if emVal == "store" then
		tile = "33a77f587f2a4018b79cae30d286db9d"
		background = "72f8b2d517384f67a32c39d889e2738e" 
	end
	
	if emVal == "reward" then
		tile = "899e3eb057974c3fb82924cfc9ad3d20"
		background = "6e55e257278c4930ad10108cbd572ee0"
	end
end

if self.stage == 4 then
	if emVal == "battle" then
		tile = "4da78b1040744461bb0907767b7eac4c"
		background = "91fde791d0bc40eea1796087fcafcf91"
	end
	
	if emVal == "puzzle" then
		tile = "4da78b1040744461bb0907767b7eac4c"
		background = "91fde791d0bc40eea1796087fcafcf91" 
	end
	
	if emVal == "event" then
		tile = "099eb7d77d184c34908fcef37918462e"
		background = "8110bb7f4ab049c7b64b128ffc6fc9a4" 
	end
	
	if emVal == "store" then
		tile = "4da78b1040744461bb0907767b7eac4c"
		background = "91fde791d0bc40eea1796087fcafcf91" 
	end
	
	if emVal == "reward" then
		tile = "4da78b1040744461bb0907767b7eac4c"
		background = "29537f30f5c045f4864d73f0c562fcd3"
	end
	
	if emVal == "entry" then
		tile = "4da78b1040744461bb0907767b7eac4c"
		background = "91fde791d0bc40eea1796087fcafcf91"
	end
end

if self.stage == 5 then
	if emVal == "boss" then
		tile = "5aec884cdec24ce2b49785d8d1440366"
		background = "34e120f5d5f34cd485a06c91351d28cc"
	end
	
	if emVal == "reward" then
		tile = "5aec884cdec24ce2b49785d8d1440366"
		background = "074d84bc389a4ae4affe18fba71e14a2"
	end
	
	if emVal == "entry" then
		tile = "5aec884cdec24ce2b49785d8d1440366"
		background = "e8429afc05ab4c3c9988301b29fb29e3"
	end
end

tileRUID.TileMapComponent.TileSetRUID = tile
backgroundRUID.BackgroundComponent:ChangeBackgroundByTemplateRUID(background)
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void ShowRound()
{
local Round = _EntityService:GetEntityByPath('/ui/RoundStart')
local Sprite = _EntityService:GetEntityByPath('/ui/RoundStart/UISprite')
Sprite.TextComponent.Text = 'STAGE ' .. tostring(math.tointeger(self.stage))
Round:SetEnable(true)
local ShowTween = _TweenLogic:MakeTween(0, 1, 2.5, EaseType.Linear, function(tweenValue)
	Sprite.SpriteGUIRendererComponent.Color.a = tweenValue
		Sprite.TextComponent.FontColor.a = tweenValue
	Sprite.TextComponent.OutlineColor.a = tweenValue
end)
ShowTween.AutoDestroy = true
local HideTween = _TweenLogic:MakeTween(1, 0, 0.5, EaseType.Linear, function(tweenValue)
	Sprite.SpriteGUIRendererComponent.Color.a = tweenValue
	Sprite.TextComponent.FontColor.a = tweenValue
	Sprite.TextComponent.OutlineColor.a = tweenValue
end)
ShowTween:SetOnEndCallback(function()
	_TimerService:SetTimerOnce(function()
		HideTween:Play()
	end, 1.5)
end)
HideTween:SetOnEndCallback(function()
	Round:SetEnable(false)
	wait(0.5)
	
	self:TotalCount()
	self:ShowMap()
	self:OldMapboard()
	self:SetCorridorInfo()
	self:SetMapInfo()
	self:CurrentMap()
	self:SetRandomBattleCorridor()
end)
HideTween.AutoDestroy = true
ShowTween:Play()
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void FavoriteDoll()
{
if _UserService.LocalPlayer.Passive:Listcheck(22) then
	local Ivt = _EntityService:GetEntityByPath("/ui/InventoryGroup")
	Ivt.InventoryManager.Coin = Ivt.InventoryManager.Coin + 5
end
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void OldMapboard()
{
if _UserService.LocalPlayer.Passive:Listcheck(33) then
	if self.oldMapItem == false then
		self.oldMapItem = true
		self.oldMapCount = 2
	end
end
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void MapReset()
{
for i = 1, self.totalMapCount, 1 do
	log(i)
	self.state[i] = false
	log(self.state[i])
	self.spawnedMaps[i]:Destroy()
	--self.spawnedMaps[i] = nil
	self.mapIDs[i] = nil
	self.values[i] = nil
	self.passedMaps[i] = nil
end

for j = 1, self.totalCorridorCount, 1 do
	if self.ranBatMark[j] ~= nil then
		self.ranBatMark[j]:Destroy()
	end
	self.corridorIDs[j]:Destroy()
	self.ranBatMark[j] = nil
	self.corridorRBs[j] = false
end

self.state = {}
self.generated = false
self.mapCount = 0
self.setPositionX = 0
self.setPositionY = 0
self.value = ""
self.beforeMapID = 0
self.currentMapID = 0
self.spawnedMap = nil
self.destination = ""
self.corridorRB = 0
self.entryMap = nil
self.currentPosX = 0
self.currentPosY = 0
self.oldMapItem = false
self.oldMapCount = 0

if self.locationMark ~= nil then
self.locationMark:Destroy()
self.locationMark = nil
end
log("여기까지 실행됨")
}
--@ EndMethod

--@ BeginEntityEventHandler
--@ Scope=Client
--@ Target=localPlayer
--@ EventName=TriggerEnterEvent
HandleTriggerEnterEvent
{
-- Parameters
local TriggerBodyEntity = event.TriggerBodyEntity
--------------------------------------------------------
if TriggerBodyEntity.Name ~= 'StopPoint' then return end

if TriggerBodyEntity.StopPoint.ToOpen == 'Map' then
if self.generated == false then
	self.generated = true
		
	--[[if self.stage > 0 then
		self:MapReset()
	end]]
	self.stage = self.stage + 1
	--[[	
	if self.stage == 6 then
		_TeleportService(_UserService.LocalPlayer,'/maps/Ending/SpawnLocation')
	end
	]]--	
	--[[self:ShowMap()
	self:SetCorridorInfo()
	self:SetMapInfo()
	self:CurrentMap()
	self:SetRandomBattleCorridor()]]
	self:ShowRound()

else return
end
end
}
--@ EndEntityEventHandler

--@ BeginEntityEventHandler
--@ Scope=Client
--@ Target=model:48ffa2a1-ef6f-4c95-a247-06ad0a0602bf
--@ EventName=ButtonClickEvent
HandleButtonClickEvent
{
-- Parameters
local Entity = event.Entity
--------------------------------------------------------
local player = _UserService.LocalPlayer

if Entity.SpawnedMapInfo.mapID == self.currentMapID then
	return
end

--self:ActivateMap()
self.spawnedMap = Entity
self:GetValue()

self.beforeMapID = self.currentMapID
self.currentMapID = math.floor(Entity.SpawnedMapInfo.mapID)
self.currentMapValue = Entity.SpawnedMapInfo.value
self.currentPosX = Entity.UITransformComponent.Position.x
self.currentPosY = Entity.UITransformComponent.Position.y

player.BattleTurnManager.isRandom = false
self:GoRandomBattle()

local mapUIEntity = _EntityService:GetEntityByPath("/ui/MapGroup/UISprite")
local Screen = _EntityService:GetEntityByPath('/ui/MapChangeScreen/UISprite')
local tweenfx = function(tweenValue)
	mapUIEntity.UITransformComponent.Position.y = tweenValue
end
local mapclose = _TweenLogic:MakeTween(0, -1000, 0.75, EaseType.BackEaseIn, tweenfx)
mapclose.AutoDestroy = true
mapclose:SetOnEndCallback(function()
	mapUIEntity:SetEnable(false) 
	player:SendEvent(StartWalkEvent())
	Screen:SetEnable(true)
	Screen.ScreenManager:ScreenOn()
	_TimerService:SetTimerOnce(function()
		_TeleportService:TeleportToEntityPath(player, self.destination)
		--player:SendEvent(StopWalkEvent())
		_ChangeLogic:StopWalk()
		player:SendEvent(GoMapEvent(self.spawnedMap.SpawnedMapInfo.value))
	end, 3)
end)
mapclose:Play()

wait(3)
--log(math.floor(self.spawnedMap.SpawnedMapInfo.mapID), self.spawnedMap.SpawnedMapInfo.value)

--log('바뀜', self.currentPosX, self.currentPosY)

--log('currentMap:',math.floor(self.currentMapID))
if self.stage < 5 then
	self:EntryMap()
end
self:UnactivateMap()
self.passedMaps[Entity.SpawnedMapInfo.mapID] = Entity
if self.stage < 5 then
	self:PassedCorridor()
end
self:RandomBattleCorridor()
self:CurrentMap()
}
--@ EndEntityEventHandler

--@ BeginEntityEventHandler
--@ Scope=Client
--@ Target=localPlayer
--@ EventName=EntityMapChangedEvent
HandleEntityMapChangedEvent
{
-- Parameters
local NewMap = event.NewMap
local OldMap = event.OldMap
local Entity = event.Entity
--------------------------------------------------------
local newMap = NewMap.Children
local tile = nil
local background = nil

for i, child in pairs(newMap) do
	if child.Name == "TileMap" then 
		tile = child
	end
	
	if child.Name == "Background" then 
		background = child
	end
end
self:MapBackNTile(tile, background)
}
--@ EndEntityEventHandler

