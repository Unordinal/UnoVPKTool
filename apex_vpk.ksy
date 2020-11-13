meta:
  id: apex_vpk
  file-extension: vpk
  endian: le

seq:
- id: magic
  contents: [0x34, 0x12, 0xAA, 0x55]
- id: version_major
  contents: [0x02, 0x00]
- id: version_minor
  contents: [0x03, 0x00]
- id: tree_size
  type: u4
- id: file_data_section_size
  type: u4
- id: node_ext
  type: node_ext

types:
  szutf8:
    seq:
    - id: value
      type: strz
      encoding: UTF-8
      
  node_ext:
    seq:
    - id: name
      type: szutf8
    - id: path_nodes
      type: node_path
      repeat: eos
  node_path:
    seq:
    - id: name
      type: szutf8
    - id: file_nodes
      type: node_file
      if: name.value.length != 0
      repeat: until
      repeat-until: _.name.value.length == 0
  node_file:
    seq:
    - id: name
      type: szutf8
    - id: entry_block
      type: entry_block
      if: name.value.length != 0
      
  entry:
    seq:
    - id: data_id
      type: u4
    - id: unk1
      type: u2
    - id: offset
      type: u4
    - id: unk2
      type: u4
    - id: length
      type: u4
    - id: unk3
      type: u4
    - id: file_size
      type: u4
    - id: unk4
      type: u4
  entry_block:
    seq:
    - id: crc
      type: u4
    - id: preload_bytes
      type: u2
    - id: archive_index
      type: u2
    #- id: entries
    #  type: entry
    #  repeat: until
    #  repeat-until: term == 0xFFFF
    - id: term
      type: u2
      repeat: until
      repeat-until: _ == 0xFFFF