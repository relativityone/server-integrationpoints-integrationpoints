import os;
import datetime;

class Entry:
	def __init__(self, time, name):
		self.time = time
		self.number = name

def Get_Path_to_Newest_LDAP_Install_Folder():
	'''
        Gets the path to the Folder containing the newest Method Installation.
        '''
	list = []
	dir = "\\\\BLD-PKGS.kcura.corp\\Packages\\\IntegrationPoints\\default\\"
	
	for folder in os.listdir(dir):
		modifedtime = os.path.getctime(os.path.join( dir + folder))
		list.append(Entry(modifedtime, folder));

	list.sort(key=lambda x: x.time)
	return os.path.join(dir, list[-1].number)

def Get_Path_to_Newest_Integration_Points():
	'''
        Gets the path to the newest Integration points .rap file.
        '''
	list = []
	folder = Get_Path_to_Newest_LDAP_Install_Folder()
	return os.path.join(folder, "RelativityIntegrationPoints.Auto.rap")
