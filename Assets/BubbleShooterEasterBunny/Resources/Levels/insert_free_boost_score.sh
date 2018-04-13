for i in {1..32}
do
  a=$(awk '{if (NR==5) print $2;}' $i.txt|cut -d\/ -f 2)
  line="FREE BOOST SCORE $a"
  sed -i.bak "6i\\
$line
" $i.txt
done
