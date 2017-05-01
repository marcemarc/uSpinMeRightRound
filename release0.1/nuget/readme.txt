 _                                                                                          
| |                                                                                         
| |_ ___   ___   ___  _ __ __ _ _ __   __ _  ___ _   _                                      
| __/ _ \ / _ \ / _ \| '__/ _` | '_ \ / _` |/ _ \ | | |                                     
| || (_) | (_) | (_) | | | (_| | | | | (_| |  __/ |_| |_                                    
 \__\___/ \___/ \___/|_|  \__,_|_| |_|\__, |\___|\__, (_)                                   
                                       __/ |      __/ |                                     
                                      |___/      |___/                                      
       _____       _      ___  ___    ______ _       _     _  ______                      _ 
      /  ___|     (_)     |  \/  |    | ___ (_)     | |   | | | ___ \                    | |
 _   _\ `--. _ __  _ _ __ | .  . | ___| |_/ /_  __ _| |__ | |_| |_/ /___  _   _ _ __   __| |
| | | |`--. \ '_ \| | '_ \| |\/| |/ _ \    /| |/ _` | '_ \| __|    // _ \| | | | '_ \ / _` |
| |_| /\__/ / |_) | | | | | |  | |  __/ |\ \| | (_| | | | | |_| |\ \ (_) | |_| | | | | (_| |
 \__,_\____/| .__/|_|_| |_\_|  |_/\___\_| \_|_|\__, |_| |_|\__\_| \_\___/ \__,_|_| |_|\__,_|
            | |                                 __/ |                                       
            |_|                                |___/                                        





===============================================================================================

A package to add a rotate option to the media section of Umbraco for images.

Blog post here:

Install of this package should add the following config to your /config/imageprocessor/processor.config file

 <plugin name="Pirouette" type="tooorangey.uSpinMeRightRound.Processors.Pirouette, tooorangey.uSpinMeRightRound" enabled="true" />

if you are running an earlier version of Umbraco (which this is completely untested on) then you may not have the image processor: processor.config file in your solution, you can add it via Nuget here: Install-Package ImageProcessor.Web.Config

Source is here: https://github.com/marcemarc/uSpinMeRightRound

An demo video is here: https://www.flickr.com/photos/marcemarc/34052092631
