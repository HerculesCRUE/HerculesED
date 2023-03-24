delete from DB.DBA.load_list;
ld_dir ('dumps', '%', NULL); 
rdf_loader_run();
checkpoint;
checkpoint_interval(60);