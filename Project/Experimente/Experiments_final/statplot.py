# -*- coding: utf-8 -*-
"""
Created on Sat Jan 16 23:27:00 2016

@author: Moe
"""

import sys
import os
import numpy as np
import matplotlib.pyplot as plt

def plot_data_from_file(stat_file, marker_color):
    data = np.genfromtxt(stat_file,skip_header=1, dtype=np.int, delimiter=';')
    x,y = (data[:,0], data[:,1])
    mean = np.empty(x.shape)
    mean.fill(np.mean(y))
    plt.plot(x, y, c=marker_color, label=stat_file)
    plt.plot(x, mean, c=marker_color, linestyle=':')


marker_colors = ['b', 'g', 'r', 'c', 'm', 'k']    

if (len(sys.argv) < 2): 
	print "No data to plot given."
else:
    files = [f for f in sys.argv[1:] if os.path.isfile(f)]
    legend_entries = []
    for i, f in enumerate(files):
        marker = marker_colors[min(i, len(marker_colors) -1)]
        plot_data_from_file(f, marker)
    
    plt.legend()
    plt.show()
