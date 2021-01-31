curVer = 2.3

PerformHttpRequest("https://raw.githubusercontent.com/ChonkyBadger/BadgerEssentials-v2/master/data.json", function(err, rawData, headers)
	local data = json.decode(rawData)
    latestVerAsNum = tonumber(data.version)
    
    if curVer == latestVerAsNum then
        print ("^4[BadgerEssentialsv2] ^3is fully up to date, version ^1" .. latestVerAsNum)
    else
        print ("^4[BadgerEssentialsv2] ^3is outdated. Version is ^1" .. curVer .. " ^3and the latest available version is ^1" .. latestVerAsNum)
        print ("^4[BadgerEssentialsv2] ^3You can download the latest version at ^1https://github.com/ChonkyBadger/BadgerLEOEssentials")
        print ("^3Changelog for version ^1" .. latestVerAsNum .. ":")
        print (data.changelog)
    end
end)
