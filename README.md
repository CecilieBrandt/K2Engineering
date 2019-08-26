K2Engineering
=============
![K2Eng](images/K2Engineering.png)

This plugin contains a set of customised Kangaroo2 grasshopper components with the scope of calibrating a number of goals with regard to structural properties.

This is particularly useful for the analysis of form-active structures (including cablenets and gridshells) that are typically characterised by their large deformations when subjected to external loads. The underlying position-based dynamics implemented in the Kangaroo2 solver inherently deals with this non-linear behaviour. This means that both form-finding and analysis can be performed within the Grasshopper environment using K2 and K2Engineering.


Installation
============
Download the latest .gha file under [releases](https://github.com/CecilieBrandt/K2Engineering/releases), unblock it and place it in your Grasshopper library folder: C:\Users\XX\AppData\Roaming\Grasshopper\Libraries

K2Engineering has the following dependencies:

[Kangaroo2](https://github.com/Dan-Piker/K2Goals): Kangaroo2 is a native part of Rhino 6 so it is automatically installed. You will most likely find it in this location: C:\Program Files\Rhino 6\Plug-ins\Grasshopper\Components

[Plankton](https://github.com/meshmash/Plankton/releases): Plankton is a halfedge mesh library and has to be installed manually. Download the latest dll and .gha files from the link, unblock them and place them in your Grasshopper library folder: C:\Users\XX\AppData\Roaming\Grasshopper\Libraries


Disclaimer
==========
Even though the plugin produces accurate and meaningful structural results, the author cannot be held responsible for the output and it should therefore always be used in combination with another finite element analysis package for validation and documentation.


Feedback and enhancements
=========================
Please report any bugs or requests for enhancement in the GitHub issue tracker.


License
=======
This software is licensed under the Apache 2 license: http://www.apache.org/licenses/LICENSE-2.0


Acknowledgement
===============
Thanks to Daniel Piker for the amazing Kangaroo2 plugin.

Thanks to James Solly for continuous dialogue and help improving the plugin.

Thanks to Format Engineers for a keen interest in applying the tool to relevant projects and thereby guiding the development.

Also thanks to Anders Deleuran, Gregory Quinn, Harri Lewis and Will Pearson for valuable inspiration and discussions.