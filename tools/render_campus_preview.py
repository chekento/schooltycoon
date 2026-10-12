"""Blender render of the same C# model recipes. Not a Unity gameplay screenshot."""
import bpy,json,math,sys
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:];data=json.load(open(args[0]));output=args[1];closeup=len(args)>2 and args[2]=='progression'
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
materials={};meshes={}
def mat(p):
 key=(p['Color'],p['Surface'])
 if key in materials:return materials[key]
 h=p['Color'];rgb=[int(h[i:i+2],16)/255 for i in (0,2,4)];rgb=[v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4 for v in rgb]
 m=bpy.data.materials.new('Campus '+h);m.diffuse_color=(*rgb,1);m.use_nodes=True;s=m.node_tree.nodes.get('Principled BSDF');s.inputs['Base Color'].default_value=(*rgb,1);s.inputs['Roughness'].default_value=.47;s.inputs['Metallic'].default_value=.32 if p['Surface']==2 else 0
 if p['Surface'] in (3,6):s.inputs['Roughness'].default_value=.21;s.inputs['Transmission Weight'].default_value=.16
 if p['Surface']==7:s.inputs['Emission Color'].default_value=(*rgb,1);s.inputs['Emission Strength'].default_value=.12
 materials[key]=m;return m
for i,p in enumerate(data):
 key=(p['Shape'],p['W'],p['H'],p['D'],p['Color'],p['Surface'])
 if key in meshes:obj=bpy.data.objects.new(p['Name'],meshes[key]);bpy.context.collection.objects.link(obj)
 else:
  shape=p['Shape']
  if shape==0:bpy.ops.mesh.primitive_cube_add(size=1)
  elif shape==2:bpy.ops.mesh.primitive_cylinder_add(vertices=16,radius=.5,depth=1)
  else:bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=8,radius=.5)
  obj=bpy.context.object;obj.name=p['Name'];obj.scale=(p['W'],p['D'],p['H']);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
  if shape==0:
   mod=obj.modifiers.new('Soft bevel','BEVEL');mod.width=min(.055,min(p['W'],p['H'],p['D'])*.21);mod.segments=2;bpy.ops.object.modifier_apply(modifier=mod.name)
   obj.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
  else:
   for poly in obj.data.polygons:poly.use_smooth=True
  obj.data.materials.append(mat(p));meshes[key]=obj.data
 obj.location=(p['X'],p['Z'],p['Y']);obj.rotation_euler[2]=math.radians(-p['Rotation'])
 if i%1000==0:print('Model parts',i,flush=True)
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True;scene.render.resolution_x=1600;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.55,.69,.61,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.5
bpy.ops.object.light_add(type='SUN',location=(5,-8,20));sun=bpy.context.object;sun.rotation_euler=(.65,-.4,-.5);sun.data.energy=2.1;sun.data.angle=.18;sun.data.color=(1,.89,.74)
bpy.ops.object.light_add(type='AREA',location=(10,-5,20));light=bpy.context.object;light.data.energy=1500;light.data.shape='DISK';light.data.size=18
bpy.ops.object.camera_add(location=(13,-12,11) if closeup else (40,-25,32));camera=bpy.context.object;direction=(Vector((4.5,1.5,1)) if closeup else Vector((15,10,2.8)))-camera.location;camera.rotation_euler=direction.to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=11 if closeup else 44;scene.camera=camera
scene.view_settings.view_transform='AgX';scene.view_settings.look='AgX - Medium High Contrast';scene.render.image_settings.file_format='JPEG' if output.lower().endswith('.jpg') else 'PNG';scene.render.image_settings.quality=88;scene.render.filepath=output;bpy.ops.render.render(write_still=True)
